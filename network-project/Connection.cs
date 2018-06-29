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
        string id, clientId = "-1", serverAddress = "google.com";
        bool isServerFirstTime = true, isServerConnected = false;

        ConnectionInfo ci;
        List<HttpRequestCache> cache;
        Thread clientThread, serverThread;
        TcpClient clientTcp, serverTcp;

        public Connection(ConnectionInfo ci, string id) {
            this.ci = ci;
            this.id = id;

            cache = new List<HttpRequestCache>();
            clientThread = ci.sourceType == ConnectionType.udp ? new Thread(udpClientReceive) : new Thread(tcpClientReceive);
            clientThread.Start();
        }

        void udpClientReceive() {

            UdpClient listener = new UdpClient(ci.sourcePort);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Parse(ci.sourceAddress), ci.sourcePort);

            try {
                while (isRun) {
                    tools.print("[udp client] waiting for a client to connect");
                    byte[] bytes = listener.Receive(ref groupEP);

                    string message = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
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
                                        new Thread(() => sendUdp(IPAddress.Parse(ci.sourceAddress), c.response, DestinationType.client)).Start();
                                    } else {
                                        new Thread(() => sendTcp(c.response, DestinationType.client)).Start();
                                    }
                                    isCache = true;
                                    break;
                                }
                            }
                            if (isCache) return;
                            serverAddress = info[3];
                            serverPort = 80;
                            cache.Add(new HttpRequestCache(serverAddress));
                            bytes = Encoding.ASCII.GetBytes($"GET / HTTP / {info[2]}\r\n\r\n");
                        }

                        if (ci.destType == ConnectionType.tcp) {
                            if (isServerConnected == false && isServerFirstTime) {
                                isServerFirstTime = false;
                                serverThread = new Thread(tcpServerReceive);
                                serverThread.Start();
                            }
                            new Thread(() => sendTcp(bytes, DestinationType.server)).Start();
                        } else {
                            if (isServerConnected == false && isServerFirstTime) {
                                isServerFirstTime = false;
                                serverThread = new Thread(udpServerReceive);
                                serverThread.Start();
                            }
                            new Thread(() => sendUdp(IPAddress.Parse(serverAddress), bytes, DestinationType.server)).Start();
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

                    if (serverPort == 80) {
                        cache[cache.Count - 1].response = bytes;
                    }

                    if (ci.sourceType == ConnectionType.udp) {
                        new Thread(() => sendUdp(IPAddress.Parse(ci.sourceAddress), bytes, DestinationType.client)).Start();
                    } else {
                        new Thread(() => sendTcp(bytes, DestinationType.client)).Start();
                    }

                    tools.print($"[tcp server] received message from server \n {Encoding.ASCII.GetString(bytes, 0, bytesRead)}");
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
                    string message = Encoding.ASCII.GetString(bytes, 0, bytesRead);
                    string[] info = message.Split('|');

                    if (info.Length > 2) {
                        string tempId = info[0].Split(':')[1];
                        if (info[0].StartsWith("id")) {
                            clientId = clientId == "-1" ? info[0].Split(':')[1] : clientId;
                        }
                        if (clientId != tempId) { clientTcp = null; return; }

                        if (info[1] == "DNS") {
                            // handle dns
                        }
                        if (ci.destType == ConnectionType.tcp) {
                            if (isServerConnected == false && isServerFirstTime) {
                                isServerFirstTime = false;
                                serverThread = new Thread(tcpServerReceive);
                                serverThread.Start();
                            }
                            new Thread(() => sendTcp(bytes, DestinationType.server)).Start();
                        } else {
                            if (isServerConnected == false && isServerFirstTime) {
                                isServerFirstTime = false;
                                serverThread = new Thread(udpServerReceive);
                                serverThread.Start();
                            }
                            new Thread(() => sendUdp(IPAddress.Parse(serverAddress), bytes, DestinationType.server)).Start();
                        }
                    }
                    tools.print(message);
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
                    string message = Encoding.ASCII.GetString(bytes, 0, bytes.Length).ToLower().Trim();

                    if (ci.sourceType == ConnectionType.udp) {
                        new Thread(() => sendUdp(IPAddress.Parse(ci.sourceAddress), bytes, DestinationType.client)).Start();
                    } else {
                        new Thread(() => sendTcp(bytes, DestinationType.client)).Start();
                    }
                    tools.print($"[udp server] received message from {groupEP.ToString()} :\n {message}\n");

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
                tools.print($"[tcp proxy] message sent to {serverAddress}:{NetworkManager.proxyPort}");
            } catch (Exception e) {
                tools.print($"[tcp proxy] error: {e.Message}");
            }
        }

        public override string ToString() {
            return $"id:[{id}] [{ci.ToString()}]";
        }
    }
}