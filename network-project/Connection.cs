using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace network_project {
    class Connection {
        int id;
        ConnectionInfo ci;

        public Connection(ConnectionInfo ci, int id) {
            this.ci = ci;
            this.id = id;
        }

        public override string ToString() {
            return $"id:[{id}] [{ci.ToString()}]";
        }
    }
}
