namespace network_project {
    class HttpRequestCache {
        public string address, data;
        public byte[] response;

        public HttpRequestCache(string address, string data) {
            this.address = address;
            this.data = data;
        }
    }
}