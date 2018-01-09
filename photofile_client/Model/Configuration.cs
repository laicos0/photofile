using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace photofile_client.Model {
    class Configuration {
        public string KeyStoneUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string PhotoDir { get; set; }
        public string LocalContentsDir { get; set; }
    }
}
