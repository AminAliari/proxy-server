using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace dns_client {
    class Program {
        static void Main(string[] args) {
            string id = getTimestamp(DateTime.Now);
            ///// tcp client
            bool found = false;
            TcpClient serverTcp = new TcpClient();
            //while (!found) {
            try {
                Console.WriteLine("connecting");
                serverTcp = new TcpClient("google.com", 80);
                Console.WriteLine("connected");
                found = true;
            } catch {

            }
            //}
            try {

                NetworkStream ns = serverTcp.GetStream();
                Console.WriteLine("sending");
                string type = "", server = "", target = "";
                byte[] sendbuf = Encoding.ASCII.GetBytes($"id:{id}|DNS|{type}|{server}|{target}");
                ns.Write(sendbuf, 0, sendbuf.Length); // send
                Console.WriteLine("sent");

            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            try {

                NetworkStream ns = serverTcp.GetStream();


                while (true) {
                    byte[] bytes = new byte[1024];
                    Console.WriteLine("reading");
                    while (!ns.DataAvailable) { }
                    int bytesRead = ns.Read(bytes, 0, bytes.Length); // read
                    File.WriteAllBytes(@"C:\Users\Amin\Desktop\response.html", bytes);
                    Console.WriteLine("read");
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
