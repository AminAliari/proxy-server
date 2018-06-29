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

    enum DestinationType {
        client,
        server
    }
}
