using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace http_client {
    class Program {
        static void Main(string[] args) {
            string id = getTimestamp(DateTime.Now);
            string version = "1.0";
            int port = 7878, proxyPort = 7879;
            
            string address = "yahoo.com";
            Console.WriteLine("my id: " + id);

            /////// udp client
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPAddress broadcast = IPAddress.Parse("192.168.1.160");

            byte[] sendbuf = Encoding.ASCII.GetBytes($"id:{id}|HTTP|{version}|{address}");
            IPEndPoint ep = new IPEndPoint(broadcast, port);

            s.SendTo(sendbuf, ep);

            Console.WriteLine($"[udp proxy] message sent to port {port}");

            bool done = false;

            UdpClient listener = new UdpClient(7879);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, proxyPort);

            try {
                while (!done) {
                    Console.WriteLine("Waiting for server");
                    byte[] bytes = listener.Receive(ref groupEP);
                    string message = Encoding.ASCII.GetString(bytes, 0, bytes.Length).ToLower().Trim();
                    try {
                        message = message.Remove(0, message.IndexOf("<"));
                        File.WriteAllText(@"C:\Users\Amin\Desktop\response.html", message);
                  
                    } catch { }
                    Console.WriteLine("Received broadcast from {0} :\n {1}\n", groupEP.ToString(), message);
                }

            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            } finally {
                listener.Close();
            }

            s.Close();
            Console.Read();
        }
        public static string getTimestamp(DateTime value) {
            return value.ToString("yyyyMMddHHmmssffff");
        }
    }
}
