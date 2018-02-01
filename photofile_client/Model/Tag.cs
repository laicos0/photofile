using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace photofile_client.Model {
    [Serializable]
    public class Tag {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
        [JsonProperty(PropertyName = "createat")]
        public DateTime CreateAt { get; set; }

        public override string ToString() => $"{Name} {Description}";
    }
}
