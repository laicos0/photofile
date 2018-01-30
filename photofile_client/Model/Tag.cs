using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace photofile_client.Model {
    public class Tag {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreateAt { get; set; }

        public override string ToString() => $"[{Name}] {Description}";
    }
}
