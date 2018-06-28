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

        int serverPort = 80;
        string id, serverAddress = "google.com";

        ConnectionInfo ci;
        Thread clientThread, serverThread, udpThread, tcpThread;
        TcpClient clientTcp, serverTcp;

        public Connection(ConnectionInfo ci, string id) {
            this.ci = ci;
            this.id = id;

            clientThread = ci.sourceType == ConnectionType.udp ? new Thread(udpClientReceive) : new Thread(tcpClientReceive);
            serverThread = ci.destType == ConnectionType.udp ? new Thread(udpServerReceive) : new Thread(tcpServerReceive);

            clientThread.Start();
            serverThread.Start();
        }

        void udpClientReceive() {

            UdpClient listener = new UdpClient(ci.sourcePort);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Parse(ci.sourceAddress), ci.sourcePort);

            try {
                while (isRun) {
                    Console.WriteLine("[udp client] waiting");
                    byte[] bytes = listener.Receive(ref groupEP);

                    string message = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                    Console.WriteLine("[udp client] received from {0} :\n {1}\n", groupEP.ToString(), message);
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

        void udpServerReceive() {
            UdpClient listener = new UdpClient(serverPort);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, serverPort);

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

        void tcpServerReceive() {
            try {
                serverTcp = new TcpClient(serverAddress, serverPort);

                NetworkStream ns = serverTcp.GetStream();
                while (isRun) {
                    byte[] bytes = new byte[1024];
                    int bytesRead = ns.Read(bytes, 0, bytes.Length);
                    Console.WriteLine(Encoding.ASCII.GetString(bytes, 0, bytesRead));
                }
                serverTcp.Close();
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        void sendUdp(string data, int port) {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPAddress broadcast = IPAddress.Parse("192.168.1.255");

            byte[] sendbuf = Encoding.ASCII.GetBytes($"{id}|{data}");
            IPEndPoint ep = new IPEndPoint(broadcast, port);

            s.SendTo(sendbuf, ep);

            Console.WriteLine($"[udp proxy] message sent to port {port}");
        }

        void sendTcp(string data, TcpClient dest) {
            try {

                NetworkStream ns = dest.GetStream();

                byte[] sendbuf = Encoding.ASCII.GetBytes($"{id}|{data}");
                ns.Write(sendbuf, 0, sendbuf.Length);

            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        public override string ToString() {
            return $"id:[{id}] [{ci.ToString()}]";
        }
    }
}