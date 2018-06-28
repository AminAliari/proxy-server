using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace network_project {

    class NetworkManager {

        List<Connection> connections;

        public NetworkManager() {
            connections = new List<Connection>();

            tools.print("welcome to AminRises/AryaNemidoonest Proxy Manager!\n");

            do {
                tools.print("\n1.enter command  2.active connections  3.help  4.exit\n");
            } while (fetch() != '4');
        }

        public char fetch() {
            char readKey = tools.readKey();

            switch (readKey) {
                case '1':
                    handleCommand();
                    break;

                case '2':
                    showConnections();
                    break;

                case '3':
                    tools.print("   command example: {proxy –s=udp:127.0.0.1:80 –d=tcp}");
                    break;

                case '4':
                    tools.print("   exiting...");
                    break;

                default:
                    tools.print("   unknown command");
                    break;
            }

            return readKey;
        }

        public void handleCommand() {

            tools.print("   = ", true);
            string command = tools.read().ToLower().Trim();

            if (!command.StartsWith("proxy")) {
                tools.print("   wrong format"); return;
            }


            string[] info = command.Split('=');

            if (info.Length != 3) {
                tools.print("   wrong format"); return;

            } else {

                string[] sourceInfo = info[1].Trim().Split(':');

                try {
                    connections.Add(new Connection(new ConnectionInfo(Int32.Parse(sourceInfo[2].Split('-')[0].Trim()), sourceInfo[1].Trim(), sourceInfo[0].Trim() == "udp" ? ConnectionType.udp : ConnectionType.tcp, info[2].Trim() == "udp" ? ConnectionType.udp : ConnectionType.tcp), connections.Count));

                } catch (Exception e) {

                    if (e.Message.Contains("connection")) {
                        tools.print("   " + e.Message);
                    } else {
                        tools.print("   wrong format"); return;
                    }
                }
            }
        }

        public void showConnections() {
            if (connections.Count > 0) {
                tools.print("   connections:");
                foreach (Connection c in connections) {
                    tools.print("      " + c);
                }
            }else {
                tools.print("   no connections");
            }
        }
    }
}