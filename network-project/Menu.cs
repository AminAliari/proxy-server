using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace network_project {

    class NetworkManager {
        public void initMenu() {
            tools.print("1.enter command\t2.active connections 3.help 4.exit");
            while (fetch() != "4") {

            }
        }

        public string fetch() {
            string readKey = tools.read();

            switch (readKey) {
                case "1":
                    handleCommand();
                    break;

                case "2":
                    showConnections();
                    break;

                case "3":
                    tools.print("command format: {proxy –s sourceProtocol:sourceHost:sourcePort –d destProtocol}");
                    break;

                case "4":
                    tools.print("exiting...");
                    break;

                default:
                    tools.print("unknown command");
                    break;
            }

            return readKey;
        }

        public void handleCommand() {

        }

        public void showConnections() {

        }
    }
}