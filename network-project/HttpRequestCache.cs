namespace network_project {
    class HttpRequestCache {
        public string address;
        public byte[] response;

        public HttpRequestCache(string address) {
            this.address = address;
        }
    }
}