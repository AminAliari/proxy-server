using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace network_project {
    class Connection {
        public bool isRun = true;

        int proxyPort = 7879, serverPort = 80;
        string id, clientId = "-1", serverAddress = "google.com";
        bool isServerFirstTime = true, isServerConnected = false;

        ConnectionInfo ci;
        Thread clientThread, serverThread;
        TcpClient clientTcp, serverTcp;

        public Connection(ConnectionInfo ci, string id) {
            this.ci = ci;
            this.id = id;

            clientThread = ci.sourceType == ConnectionType.udp ? new Thread(udpClientReceive) : new Thread(tcpClientReceive);
            clientThread.Start();
        }

        void udpClientReceive() {

            UdpClient listener = new UdpClient(ci.sourcePort);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Parse(ci.sourceAddress), ci.sourcePort);

            try {
                while (isRun) {
                    Console.WriteLine("[udp client] waiting");
                    byte[] bytes = listener.Receive(ref groupEP);

                    string message = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                    Console.WriteLine("[udp client] connected from {0}", groupEP.ToString());

                    string[] info = message.Split('|');
                   
                    if (info.Length > 2) {
                        string tempId = info[0].Split(':')[1];
                        if (info[0].StartsWith("id")) {
                            clientId = clientId == "-1" ? info[0].Split(':')[1] : clientId;
                        }
                        if (clientId != tempId) return;

                        if (info[1] == "HTTP") {
                            serverAddress = info[3];
                            serverPort = 80;
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
                Console.WriteLine(e.ToString());
            } finally {
                listener.Close();
            }
        }

        void tcpServerReceive() {
            try {
                serverTcp = new TcpClient(serverAddress, serverPort);
                isServerConnected = true;

                while (isRun) {
                    NetworkStream ns = serverTcp.GetStream();
                    byte[] bytes = new byte[1024];
                    while (!ns.DataAvailable) { }
                    int bytesRead = ns.Read(bytes, 0, bytes.Length);

                    new Thread(() => sendUdp(IPAddress.Parse(ci.sourceAddress), bytes, DestinationType.client)).Start(); ;
                    Console.WriteLine(Encoding.ASCII.GetString(bytes, 0, bytesRead));
                }
                serverTcp.Close();
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        void udpServerReceive() {
            UdpClient listener = new UdpClient(serverPort);
            isServerConnected = true;
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, serverPort);

            while (isRun) {
                try {

                    Console.WriteLine("Waiting for server");
                    byte[] bytes = listener.Receive(ref groupEP);
                    string message = Encoding.ASCII.GetString(bytes, 0, bytes.Length).ToLower().Trim();
                    Console.WriteLine("Received broadcast from {0} :\n {1}\n", groupEP.ToString(), message);
                } catch (Exception e) {
                    Console.WriteLine(e.ToString());
                }
            }
            listener.Close();
        }

        void tcpClientReceive() {
            bool done = false;

            TcpListener listener = new TcpListener(ci.sourcePort);

            listener.Start();
            tools.print("[tcp client] waiting");
            clientTcp = listener.AcceptTcpClient();

            tools.print("[tcp client] connection accepted.");
            try {
                NetworkStream ns = clientTcp.GetStream();
                while (isRun) {
                    byte[] bytes = new byte[1024];
                    int bytesRead = ns.Read(bytes, 0, bytes.Length);
                    Console.WriteLine(Encoding.ASCII.GetString(bytes, 0, bytesRead));
                }
                clientTcp.Close();
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            listener.Stop();
        }

        void sendUdp(IPAddress address, byte[] data, DestinationType destType) {
            if (destType == DestinationType.server) {
                while (!isServerConnected) {

                }
            }

            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPEndPoint ep = new IPEndPoint(address, proxyPort);

            s.SendTo(data, ep);

            Console.WriteLine($"[udp proxy] message sent to {address.ToString()}:{proxyPort}");
        }

        void sendTcp(byte[] data, DestinationType destType) {
            NetworkStream ns;

            if (destType == DestinationType.server) {
                while (!isServerConnected) {

                }
                ns = serverTcp.GetStream();
            } else {
                ns = clientTcp.GetStream();
            }

            try {
                ns.Write(data, 0, data.Length);

            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        public override string ToString() {
            return $"id:[{id}] [{ci.ToString()}]";
        }
    }

    enum DestinationType {
        client,
        server
    }
}