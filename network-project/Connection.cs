using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;

namespace network_project {
    class Connection {
        public bool isRun = true;

        int serverPort = 80;
        string id, clientId = "-1", oldHostname, hostname, serverAddress = "google.com";
        bool isServerFirstTime = true, isServerConnected = false;

        ConnectionInfo ci;
        List<HttpRequestCache> cache;
        TcpClient clientTcp, serverTcp;

        public Connection(ConnectionInfo ci, string id) {
            this.ci = ci;
            this.id = id;

            cache = new List<HttpRequestCache>();
            if (ci.sourceType == ConnectionType.udp) {
                new Thread(udpClientReceive).Start();
            } else {
                new Thread(tcpClientReceive).Start();
            }
        }

        void udpClientReceive() {

            UdpClient listener = new UdpClient(ci.sourcePort);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Parse(ci.sourceAddress), ci.sourcePort);

            try {
                while (isRun) {
                    tools.print("[udp client] waiting for a client to connect");
                    byte[] bytes = listener.Receive(ref groupEP);

                    string message = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                    tools.print($"[udp client] client connected from {groupEP.ToString()}");

                    string[] info = message.Split('|');

                    if (info.Length > 2) {
                        string tempId = info[0].Split(':')[1];
                        if (info[0].StartsWith("id")) {
                            clientId = clientId == "-1" ? info[0].Split(':')[1] : clientId;
                        }
                        if (clientId != tempId) return;

                        if (info[1] == "HTTP") {
                            bool isCache = false;
                            foreach (HttpRequestCache c in cache) {
                                if (c.address == info[3]) {
                                    if (ci.sourceType == ConnectionType.udp) {
                                        sendUdp(IPAddress.Parse(ci.sourceAddress), c.response, DestinationType.client);
                                    } else {
                                        sendTcp(c.response, DestinationType.client);
                                    }
                                    isCache = true;
                                    break;
                                }
                            }
                            if (isCache) return;
                            serverAddress = info[2];
                            serverPort = 80;
                            bytes = Encoding.UTF8.GetBytes(info[3]);
                            cache.Add(new HttpRequestCache(serverAddress, info[3]));

                        }

                        if (ci.destType == ConnectionType.tcp) {
                            if (isServerConnected == false && isServerFirstTime) {
                                isServerFirstTime = false;
                                new Thread(tcpServerReceive).Start();
                            }
                            sendTcp(bytes, DestinationType.server);
                        } else {
                            if (isServerConnected == false && isServerFirstTime) {
                                isServerFirstTime = false;
                                new Thread(udpServerReceive).Start();
                            }
                            sendUdp(IPAddress.Parse(serverAddress), bytes, DestinationType.server);
                        }
                    }
                }
                listener.Close();

            } catch (Exception e) {
                tools.print($"[udp client] error: {e.Message}");
            } finally {
                listener.Close();
            }
        }

        void tcpServerReceive() {
            try {
                tools.print("[tcp server] waiting to connect to server");

                serverTcp = new TcpClient(serverAddress, serverPort);
                isServerConnected = true;

                tools.print("[tcp server] connected to server");

                while (isRun) {
                    NetworkStream ns = serverTcp.GetStream();

                    byte[] bytes = new byte[1024];
                    while (!ns.DataAvailable) { }
                    int bytesRead = ns.Read(bytes, 0, bytes.Length);
                    string message = Encoding.UTF8.GetString(bytes, 0, bytesRead);

                    if (serverPort == 80) {
                        HttpRequestCache lastCache = cache[cache.Count - 1];
                        lastCache.response = bytes;

                        string[] lines = message.Split('\n');
                        if (lines[0].Contains("Moved") || lines[0].Contains("302 Found")) { // handling 301 and 302 status code

                            foreach (string l in lines) {
                                if (l.StartsWith("Location")) {

                                    Uri uri = new Uri(l.Remove(l.IndexOf("Location: "), 10).Trim());

                                    oldHostname = hostname;
                                    hostname = uri.AbsolutePath;

                                    string newData;

                                    if (string.IsNullOrEmpty(oldHostname)) {
                                        newData = lastCache.data.Insert(lastCache.data.IndexOf('/'), hostname);
                                    } else {
                                        newData = lastCache.data.Replace($"{oldHostname}", $"{hostname}");
                                    }

                                    bytes = Encoding.UTF8.GetBytes(newData);

                                    serverTcp.Close();
                                    isServerConnected = false;

                                    new Thread(tcpServerReceive).Start();

                                    if (ci.destType == ConnectionType.tcp) {
                                        sendTcp(bytes, DestinationType.server);
                                    } else {
                                        sendUdp(IPAddress.Parse(serverAddress), bytes, DestinationType.server);
                                    }
                                    break;
                                }
                            }

                            return;
                        } else if (lines[0].Contains("Not Found")) { // handling 301 and 404 status code
                            bytes = Encoding.UTF8.GetBytes("request not found (404)");

                            if (ci.sourceType == ConnectionType.udp) {
                                sendUdp(IPAddress.Parse(ci.sourceAddress), bytes, DestinationType.client);
                            } else {
                                sendTcp(bytes, DestinationType.client);
                            }
                        }
                    }

                    if (ci.sourceType == ConnectionType.udp) {
                        sendUdp(IPAddress.Parse(ci.sourceAddress), bytes, DestinationType.client);
                    } else {
                        sendTcp(bytes, DestinationType.client);
                    }

                    tools.print($"[tcp server] message received from server \n {message}");
                }
                serverTcp.Close();
            } catch (Exception e) {
                tools.print($"[tcp server] error: {e.Message}");
            }
        }

        void tcpClientReceive() {
            bool done = false;

            TcpListener listener = new TcpListener(ci.sourcePort);

            listener.Start();
            tools.print("[tcp client] waiting for client to connect");
            clientTcp = listener.AcceptTcpClient();

            tools.print("[tcp client] client connection accepted");
            try {
                NetworkStream ns = clientTcp.GetStream();
                while (isRun) {
                    byte[] bytes = new byte[1024];
                    int bytesRead = ns.Read(bytes, 0, bytes.Length);
                    string message = Encoding.UTF8.GetString(bytes, 0, bytesRead);
                    string[] info = message.Split('|');

                    if (info.Length > 2) {
                        string tempId = info[0].Split(':')[1];
                        if (info[0].StartsWith("id")) {
                            clientId = clientId == "-1" ? info[0].Split(':')[1] : clientId;
                        }
                        if (clientId != tempId) { clientTcp = null; return; }

                        if (info[1] == "DNS") {
                            // handle dns (reliable + cache + [dns-client])
                        }
                        if (ci.destType == ConnectionType.tcp) {
                            if (isServerConnected == false && isServerFirstTime) {
                                isServerFirstTime = false;
                                new Thread(tcpServerReceive).Start();
                            }
                            sendTcp(bytes, DestinationType.server);
                        } else {
                            if (isServerConnected == false && isServerFirstTime) {
                                isServerFirstTime = false;
                                new Thread(udpServerReceive).Start();
                            }
                            sendUdp(IPAddress.Parse(serverAddress), bytes, DestinationType.server);
                        }
                    }
                    tools.print($"[tcp client] message received from {ci.sourceAddress}:{ci.sourcePort}, message: {message}");
                }
                clientTcp.Close();
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }

            listener.Stop();
        }

        void udpServerReceive() {
            UdpClient listener = new UdpClient(serverPort);
            isServerConnected = true;
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, serverPort);

            while (isRun) {
                try {

                    tools.print("[udp server] waiting for server to connect");

                    byte[] bytes = listener.Receive(ref groupEP);
                    string message = Encoding.UTF8.GetString(bytes, 0, bytes.Length).ToLower().Trim();

                    if (ci.sourceType == ConnectionType.udp) {
                        sendUdp(IPAddress.Parse(ci.sourceAddress), bytes, DestinationType.client);
                    } else {
                        sendTcp(bytes, DestinationType.client);
                    }
                    tools.print($"[udp server] message received from {groupEP.ToString()} :\n {message}\n");

                } catch (Exception e) {
                    tools.print($"[udp server] error: {e.Message}");
                }
            }
            listener.Close();
        }

        void sendUdp(IPAddress address, byte[] data, DestinationType destType) {
            if (destType == DestinationType.server) {
                while (!isServerConnected) {

                }
            }

            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ep = new IPEndPoint(address, NetworkManager.proxyPort);
            s.SendTo(data, ep);

            tools.print($"[udp proxy] message sent to {address.ToString()}:{NetworkManager.proxyPort}");
        }

        void sendTcp(byte[] data, DestinationType destType) {
            NetworkStream ns;

            if (destType == DestinationType.server) {
                while (!isServerConnected) { }
                ns = serverTcp.GetStream();
            } else {
                while (clientTcp == null) { }
                ns = clientTcp.GetStream();
            }

            try {
                ns.Write(data, 0, data.Length);
                tools.print($"[tcp proxy] message sent to {serverAddress}:{(destType == DestinationType.server ? serverPort : ci.sourcePort)}");
            } catch (Exception e) {
                tools.print($"[tcp proxy] error: {e.Message}");
            }
        }

        public override string ToString() {
            return $"id:[{id}] [{ci.ToString()}]";
        }
    }
}