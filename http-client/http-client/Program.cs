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
            Console.WriteLine("my id: " + id);

            /////// udp client
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPAddress broadcast = IPAddress.Parse("192.168.1.160");

            byte[] sendbuf = Encoding.ASCII.GetBytes("GET / HTTP/1.0\r\n\r\n");
            IPEndPoint ep = new IPEndPoint(broadcast, 7878);

            s.SendTo(sendbuf, ep);

            Console.WriteLine($"[udp proxy] message sent to port {7878}");

            bool done = false;

            UdpClient listener = new UdpClient(7879);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, 7879);

            try {
                while (!done) {
                    Console.WriteLine("Waiting for server");
                    byte[] bytes = listener.Receive(ref groupEP);
                    string message = Encoding.ASCII.GetString(bytes, 0, bytes.Length).ToLower().Trim();
                    try {
                        message = message.Remove(0, message.IndexOf("<html>"));
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
