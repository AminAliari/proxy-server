using System;
using System.Net;
using System.Text;
using DNS.Protocol;
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
        List<DnsQueryCache> dnsCache;

        TcpClient clientTcp, serverTcp;

        public Connection(ConnectionInfo ci, string id) {
            this.ci = ci;
            this.id = id;

            cache = new List<HttpRequestCache>();
            dnsCache = new List<DnsQueryCache>();

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
                    tools.print("[udp client] waiting for a client to send message");
                    byte[] bytes = listener.Receive(ref groupEP);

                    string message = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                    tools.print($"[udp client] message recevied from {groupEP.ToString()}, message: {message}");

                    string[] info = message.Split('|');

                    if (info.Length > 2) {
                        string tempId = info[0].Split(':')[1];
                        if (info[0].StartsWith("id")) {
                            clientId = clientId == "-1" ? info[0].Split(':')[1] : clientId;
                        }
                        if (clientId != tempId) continue;

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
                            if (isCache) continue;
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

                    tools.print($"[tcp server] message received from server \n {message}");

                    if (serverPort == 80) {
                        HttpRequestCache lastCache = cache[cache.Count - 1];

                        string[] lines = message.Split('\n');
                        if (lines[0].Contains("Moved") || lines[0].Contains("302 Found")) { // handling 301 and 302 status code

                            foreach (string l in lines) {
                                if (l.StartsWith("Location")) {
                                    string rawAddress = l.Remove(l.IndexOf("Location: "), 10).Trim();

                                    Uri uri = new Uri(rawAddress);

                                    //if (rawAddress.ToLower().Contains("https")) {
                                    //    serverPort = 443;
                                    //}

                                    oldHostname = hostname;
                                    hostname = uri.AbsolutePath;

                                    if (uri.Host.StartsWith("www.")) {
                                        serverAddress = serverAddress.Insert(0, "www.");
                                    }

                                    string newData;

                                    if (string.IsNullOrEmpty(oldHostname)) {
                                        newData = lastCache.data.Insert(lastCache.data.IndexOf('/'), hostname);
                                    } else {
                                        newData = lastCache.data.Replace($"{oldHostname}", $"{hostname}");
                                    }
                                    newData = newData.Remove(newData.IndexOf("Host:"));
                                    newData = newData.Insert(newData.Length - 1, $"\nHost: {serverAddress}\n\n");

                                    //tools.print("->>>>>> " + oldHostname + "|" + hostname + "|" + newData);

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

                            continue;
                        } else if (lines[0].Contains("Not Found")) { // handling 301 and 404 status code
                            bytes = Encoding.UTF8.GetBytes("request not found (404)");

                            if (ci.sourceType == ConnectionType.udp) {
                                sendUdp(IPAddress.Parse(ci.sourceAddress), bytes, DestinationType.client);
                            } else {
                                sendTcp(bytes, DestinationType.client);
                            }
                        } else {
                            lastCache.response = bytes;
                        }
                    }

                    if (ci.sourceType == ConnectionType.udp) {
                        sendUdp(IPAddress.Parse(ci.sourceAddress), bytes, DestinationType.client);
                    } else {
                        sendTcp(bytes, DestinationType.client);
                    }


                }
                serverTcp.Close();
            } catch (Exception e) {
                tools.print($"[tcp server] error: {e.Message}");
            }
        }

        async void tcpClientReceive() {

            TcpListener listener = new TcpListener(ci.sourcePort);

            listener.Start();
            tools.print("[tcp client] waiting for client to connect");
            clientTcp = listener.AcceptTcpClient();

            tools.print("[tcp client] client connection accepted");
            try {
                NetworkStream ns = clientTcp.GetStream();
                while (isRun) {
                    tools.print("[tcp client] waiting for client to send message");

                    byte[] bytes = new byte[1024];
                    int bytesRead = ns.Read(bytes, 0, bytes.Length);
                    string message = Encoding.UTF8.GetString(bytes, 0, bytesRead);
                    tools.print($"[tcp client] message received from {ci.sourceAddress}:{ci.sourcePort}, message: {message}");

                    string[] info = message.Split('|');

                    if (info.Length > 2) {
                        string tempId = info[0].Split(':')[1];
                        if (info[0].StartsWith("id")) {
                            clientId = clientId == "-1" ? info[0].Split(':')[1] : clientId;
                        }
                        if (clientId != tempId) { clientTcp = null; continue; }

                        if (info[1] == "DNS") {

                            string[] param = info[2].Split(' ');
                            string target = param[2].Substring(param[2].IndexOf('=') + 1);
                            RecordType type = param[0].Substring(param[0].IndexOf('=') + 1) == "A" ? RecordType.A : RecordType.CNAME;
                            bool isCache = false;
                            foreach (DnsQueryCache c in dnsCache) {
                                if (c.target == target && c.type == type) {
                                    sendTcp(c.response, DestinationType.client);
                                    isCache = true;
                                    break;
                                }
                            }
                            if (isCache) continue;

                            Request request = new Request();

                            request.Id = 123;
                            serverPort = 53;
                            request.RecursionDesired = true;
                            request.Questions.Add(new Question(Domain.FromString(target), type, RecordClass.IN));

                            serverAddress = param[1].Substring(param[1].IndexOf('=') + 1);
                            bytes = request.ToArray();

                            UdpClient udp = new UdpClient();
                            IPEndPoint serverIPEnd = new IPEndPoint(IPAddress.Parse(serverAddress), 53);

                            await udp.SendAsync(request.ToArray(), request.Size, serverIPEnd);
                            tools.print($"[tcp client] dns query sent to {serverAddress}, target: {target}, type: {type}");

                            UdpReceiveResult result = await udp.ReceiveAsync();
                            byte[] buffer = result.Buffer;
                            Response response = Response.FromArray(buffer);

                            byte[] sendBytes = Encoding.UTF8.GetBytes(response.ToString());
                            dnsCache.Add(new DnsQueryCache(target, type, sendBytes));

                            tools.print($"[udp server] dns response recevied: {response}");

                            sendTcp(sendBytes, DestinationType.client);

                            continue;
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
                    tools.print($"[udp server] message received from {groupEP.ToString()} :\n {message}\n");

                    if (ci.sourceType == ConnectionType.udp) {
                        sendUdp(IPAddress.Parse(ci.sourceAddress), bytes, DestinationType.client);
                    } else {
                        sendTcp(bytes, DestinationType.client);
                    }

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
            int port = destType == DestinationType.server ? serverPort : NetworkManager.proxyPort;
            IPEndPoint ep = new IPEndPoint(address, NetworkManager.proxyPort);
            s.SendTo(data, ep);

            tools.print($"[udp proxy] message sent to {address.ToString()}:{port}");
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