using System;

namespace dns_client {

    class NetworkManager {

        public static bool isRun = true;
        public static int port = 7878;

        Connection c;

        public NetworkManager() {
            tools.print("welcome to AminRises/AryaNemidoonest Dns Client!\n");

            do {
                tools.print("\n1.enter command  2.active connection 3. set port 4.help  5.exit\n");
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
                    tools.print("   command example: {type=A server=217.215.155.155 target=aut.ac.ir}");
                    break;

                case '5':
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

            if (!command.StartsWith("type") || !command.Contains("server") || !command.Contains("target")) {
                tools.print("   wrong format"); return;
            }

            try {
                if (c == null) {
                    c = new Connection(command);
                } else {
                    c.data = command;
                    c.send();
                }
            } catch {
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
                if (port < 1025 || port > 65535) {
                    port = 7878;
                    throw new Exception();
                }
            } catch {
                tools.print("   wrong entry");
            }
        }
    }
}