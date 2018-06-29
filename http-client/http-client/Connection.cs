using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace http_client {
    class Connection {

        public string id, address, data;

        int responseCounter = 0;

        public Connection(string address, string data) {
            id = tools.getTimestamp(DateTime.Now);
            this.address = address;
            this.data = data;
            new Thread(run).Start();
        }

        void run() {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPAddress broadcast = IPAddress.Parse("127.0.0.1");

            byte[] sendbuf = Encoding.UTF8.GetBytes($"id:{id}|HTTP|{address}|{data}");
            IPEndPoint ep = new IPEndPoint(broadcast, NetworkManager.port);

            s.SendTo(sendbuf, ep);
            s.Close();
            tools.print($"message sent to server on port {NetworkManager.port}");

            UdpClient listener = new UdpClient(7879);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, NetworkManager.proxyPort);

            try {
                while (NetworkManager.isRun) {
                  
                    tools.print("waiting for server to send response");
                    byte[] bytes = listener.Receive(ref groupEP);
                    string message = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                    if (message.Contains("200 OK")) {
                        try {
                            message = message.Remove(0, message.IndexOf("<"));
                            File.WriteAllText($@"C:\Users\Amin\Desktop\response{responseCounter}.html", message);

                        } catch { }
                    }else {
                        File.AppendAllText($@"C:\Users\Amin\Desktop\response{responseCounter}.html", message);
                    }
                    if (message.Contains("</html>")) {
                        responseCounter++;
                    }
                    tools.print($"message received from {groupEP.ToString()} :\n {message}\n");
                }

            } catch (Exception e) {
                tools.print($"error: {e.ToString()}");
            } finally {
                listener.Close();
            }
        }

        public void send() {
            new Thread(sendUdp).Start();
        }

        private void sendUdp() {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPAddress broadcast = IPAddress.Parse("127.0.0.1");

            byte[] sendbuf = Encoding.UTF8.GetBytes($"id:{id}|HTTP|{address}|{data}");
            IPEndPoint ep = new IPEndPoint(broadcast, NetworkManager.port);

            s.SendTo(sendbuf, ep);

            tools.print($"message sent to server on port {NetworkManager.port}");

        }

        public override string ToString() {
            return $"id:[{id}] {NetworkManager.port}:udp->{address}:{NetworkManager.proxyPort}:tcp";
        }
    }
}