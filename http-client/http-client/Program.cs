using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace http_client {
    class Program {
        static void Main(string[] args) {
            string id = getTimestamp(DateTime.Now);
            Console.WriteLine("my id: " + id);
            //Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            //IPAddress broadcast = IPAddress.Parse("192.168.1.160");

            //byte[] sendbuf = Encoding.ASCII.GetBytes($"{id}|salam");
            //IPEndPoint ep = new IPEndPoint(broadcast, 7878);

            //s.SendTo(sendbuf, ep);

            //Console.WriteLine($"[udp proxy] message sent to port {7878}");

            //while (true) {
            //    Console.WriteLine("Waiting for server");
            //    byte[] data = new byte[1024];
            //    EndPoint eep = (EndPoint)ep;
            //    int receivedDataLength = s.ReceiveFrom(data, ref eep);
            //    Console.WriteLine("Message received from {0}:", ep.ToString());
            //    Console.WriteLine(Encoding.ASCII.GetString(data, 0, receivedDataLength));
            //}
            bool found = false;
            TcpClient serverTcp = new TcpClient() ;
            while (!found) {try {
                    serverTcp = new TcpClient("192.168.1.160", 65515);
                    found = true;
                }catch {

                }
            }
            try {

                NetworkStream ns = serverTcp.GetStream();

                byte[] sendbuf = Encoding.ASCII.GetBytes($"{id}|hoyy");
                ns.Write(sendbuf, 0, sendbuf.Length);

            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            try {
                NetworkStream ns = serverTcp.GetStream();
                while (true) {
                    byte[] bytes = new byte[1024];
                    int bytesRead = ns.Read(bytes, 0, bytes.Length);
                    Console.WriteLine(Encoding.ASCII.GetString(bytes, 0, bytesRead));
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
            Console.Read();
        }
        public static string getTimestamp(DateTime value) {
            return value.ToString("yyyyMMddHHmmssffff");
        }
    }
}
