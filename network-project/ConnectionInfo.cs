using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace network_project {
    class ConnectionInfo {

        public int sourcePort;
        public string sourceAddress;
        public ConnectionType sourceType;

        public ConnectionType destType;

        public ConnectionInfo(int sourcePort, string sourceAddress, ConnectionType sourceType, ConnectionType destType) {
            this.sourcePort = sourcePort;
            this.sourceAddress = sourceAddress;
            this.sourceType = sourceType;
            this.destType = destType;
        }

        public override string ToString() {
            return $"{sourceAddress}:{sourcePort}:{sourceType} -> {destType}";
        }
    }

    enum ConnectionType {
        udp,
        tcp
    }
}
