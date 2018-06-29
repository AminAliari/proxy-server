using System;

namespace http_client {

    class NetworkManager {

        public static bool isRun = true;
        public static int port = 7878, proxyPort = 7879;

        Connection c;

        public NetworkManager() {
            tools.print("welcome to AminRises/AryaNemidoonest Http Client!\n");

            do {
                tools.print("\n1.enter command  2.active connection 3. set client port 4.set proxy port 5.help  6.exit\n");
            } while (fetch() != '4');
        }

        public char fetch() {
            char readKey = tools.readKey();

            switch (readKey) {
                case '1':
                    handleCommand();
                    break;

                case '2':
                    showConnection();
                    break;

                case '3':
                    getPort();
                    break;

                case '4':
                    getProxyPort();
                    break;

                case '5':
                    tools.print("   command example: {GET / HTTP/1.1\nHost: aut.ac.ir}");
                    break;

                case '6':
                    tools.print("   exiting...");
                    isRun = false;
                    break;

                default:
                    tools.print("   unknown command");
                    break;
            }

            return readKey;
        }

        public void handleCommand() {

            tools.print("   command = ", true);
            string command = tools.read();

            if (!command.StartsWith("GET")) {
                tools.print("   wrong format"); return;
            }


            string[] info = command.Split('/');

            tools.print("             ", true);
            string hostPart = tools.read();

            if (string.IsNullOrEmpty(hostPart)) {
                tools.print("   wrong format"); return;
            }

            command = $"{command}\n{hostPart}\n\n";

            try {
                if (c == null) {
                    c = new Connection(hostPart.Split(':')[1].Trim(), command);
                } else {
                    c.address = hostPart.Split(':')[1].Trim();
                    c.data = command;
                    c.send();
                }
            } catch (Exception e) {
                tools.print("   wrong format"); return;
            }
        }

        public void showConnection() {
            if (c != null) {
                tools.print($"   connections:\n      {c}");
            } else {
                tools.print("   no connection");
            }
        }

        public void getPort() {
            tools.print($"   port (default {port}) = ", true);
            string input = tools.read().ToLower().Trim();

            try {
                int.TryParse(input, out port);
                if (port < 1025 || port > 65535 || proxyPort == port) {
                    port = 7878;
                    throw new Exception();
                }
            } catch {
                tools.print("   wrong entry");
            }
        }

        public void getProxyPort() {
            tools.print($"   proxy port (default {proxyPort}) = ", true);
            string input = tools.read().ToLower().Trim();

            try {
                int.TryParse(input, out proxyPort);
                if (proxyPort < 1025 || proxyPort > 65535 || proxyPort == port) {
                    proxyPort = 7879;
                    throw new Exception();
                }
            } catch {
                tools.print("   wrong entry");
            }
        }
    }
}