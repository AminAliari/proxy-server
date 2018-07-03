using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Timers;

namespace dns_client {
    class Connection {

        public string id, data;

        string address = "127.0.0.1";
        TcpClient serverTcp;
        System.Timers.Timer timer;

        public Connection(string data) {
            id = tools.getTimestamp(DateTime.Now);
            this.data = data;

            timer = new System.Timers.Timer(2000);
            timer.Elapsed += (sender, e) => timeOut();

            new Thread(run).Start();
        }

        void run() {
            try {
                if (serverTcp == null) {
                    serverTcp = new TcpClient(address, NetworkManager.port);
                }
                NetworkStream ns = serverTcp.GetStream();

                byte[] sendbuf = Encoding.ASCII.GetBytes($"id:{id}|DNS|{data}");
                ns.Write(sendbuf, 0, sendbuf.Length);

                timer.Start();
                tools.print($"message sent to server on port {NetworkManager.port}");

                while (NetworkManager.isRun) {
                    byte[] bytes = new byte[1024];
                    while (!ns.DataAvailable) { }

                    int bytesRead = ns.Read(bytes, 0, bytes.Length);
                    timer.Stop();
                    tools.print($"message recevied from server: {Encoding.ASCII.GetString(bytes, 0, bytesRead)}");
                }
                serverTcp.Close();

            } catch (Exception e) {
                tools.print($"there was a problem: {e.Message}");
            }
        }

        public void send() {
            new Thread(sendTcp).Start();
        }

        private void sendTcp() {
            try {
                NetworkStream ns = serverTcp.GetStream();

                byte[] sendbuf = Encoding.ASCII.GetBytes($"id:{id}|DNS|{data}");
                ns.Write(sendbuf, 0, sendbuf.Length);

                timer.Start();
                tools.print($"message sent to server on port {NetworkManager.port}");

            } catch (Exception e) {
                tools.print($"there was a problem: {e.Message}");
            }
        }

        void timeOut() {
            timer.Stop();
            NetworkManager.isRun = false;
            NetworkManager.isRun = true;
            run();
        }

        public override string ToString() {
            return $"id:[{id}] {NetworkManager.port}:tcp->{address}:{NetworkManager.port}:tcp";
        }
    }
}