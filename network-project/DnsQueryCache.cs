using DNS.Protocol;

namespace network_project {
    class DnsQueryCache {
        public string target;
        public RecordType type;
        public byte[] response;

        public DnsQueryCache(string target, RecordType type, byte[] response) {
            this.target = target;
            this.type = type;
            this.response = response;
        }
    }
}