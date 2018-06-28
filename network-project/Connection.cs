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
        string id, serverAddress = "forum.bazicenter.com";
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

                    if (isServerConnected == false && isServerFirstTime) {
                        isServerFirstTime = false;
                        serverThread = ci.destType == ConnectionType.udp ? new Thread(udpServerReceive) : new Thread(tcpServerReceive);
                        serverThread.Start();
                    }
                    new Thread(() => sendTcp(bytes, DestinationType.server)).Start();
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

                    new Thread(() => sendUdp(IPAddress.Parse(ci.sourceAddress), bytes)).Start(); ;
                    Console.WriteLine(Encoding.ASCII.GetString(bytes, 0, bytesRead));
                }
                serverTcp.Close();
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        void udpServerReceive() {
            UdpClient listener = new UdpClient(serverPort);
            IPEndPoint groupEP = new IPEndPoint(Dns.GetHostAddresses(serverAddress)[0], serverPort);

            try {
                while (isRun) {
                    Console.WriteLine("[tcp server] waiting");
                    byte[] bytes = listener.Receive(ref groupEP);

                    string message = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                    Console.WriteLine("[tcp server] received from {0} :\n {1}\n", groupEP.ToString(), message);
                }
                listener.Close();
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            } finally {
                listener.Close();
            }
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

        void sendUdp(IPAddress address, byte[] data) {

            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPEndPoint ep = new IPEndPoint(address, proxyPort);

            s.SendTo(data, ep);

            Console.WriteLine($"[udp proxy] message sent to {address.ToString()}:{proxyPort}");
        }

        void sendTcp(byte[] data, DestinationType destType) {
            while (!isServerConnected) {
               
            }
            try {
                NetworkStream ns = destType == DestinationType.client ? clientTcp.GetStream() : serverTcp.GetStream();
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