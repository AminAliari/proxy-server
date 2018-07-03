using System;
using System.Collections.Generic;

namespace network_project {

    class NetworkManager {

        public static int proxyPort = 7879;

        List<Connection> connections;

        public NetworkManager() {
            connections = new List<Connection>();

            tools.print("welcome to AminRises/AryaNemidoonest Proxy Manager!\n");

            do {
                tools.print("\n1.enter command  2.active connections 3.set proxy port 4.help  4.exit\n");
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
                    getProxyPort();
                    break;

                case '4':
                    tools.print("   command example: {proxy –s=udp:127.0.0.1:80 –d=tcp}");
                    break;

                case '5':
                    tools.print("   exiting...");
                    foreach (Connection c in connections) {
                        c.isRun = false;
                    }
                    break;

                default:
                    tools.print("   unknown command");
                    break;
            }

            return readKey;
        }

        public void handleCommand() {

            tools.print("   command = ", true);
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
                    connections.Add(new Connection(new ConnectionInfo(int.Parse(sourceInfo[2].Split('-')[0].Trim()), sourceInfo[1].Trim(), sourceInfo[0].Trim() == "udp" ? ConnectionType.udp : ConnectionType.tcp, info[2].Trim() == "udp" ? ConnectionType.udp : ConnectionType.tcp), tools.getTimestamp(DateTime.Now)));

                } catch {
                    tools.print("   wrong format"); return;
                }
            }
        }

        public void showConnections() {
            if (connections.Count > 0) {
                tools.print("   connections:");
                foreach (Connection c in connections) {
                    tools.print($"      {c}");
                }
            } else {
                tools.print("   no connections");
            }
        }

        public void getProxyPort() {
            tools.print($"   port (default {proxyPort}) = ", true);
            string input = tools.read().ToLower().Trim();

            try {
                int.TryParse(input, out proxyPort);
                if (proxyPort < 1025 || proxyPort > 65535) {
                    proxyPort = 7879;
                    throw new Exception();
                }
            } catch {
                tools.print("   wrong entry");
            }
        }
    }
}