using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pervasivecoursework
{
    [JsonObject]
    public class Value
    {
        [JsonProperty(PropertyName = "nodeId", Required = Required.Always)]
        public string nodeId { get; set; }

        [JsonProperty(PropertyName = "date")]
        public DateTime Stamp { get; set; }

        [JsonProperty(PropertyName = "value", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Val { get; set; }

        [JsonProperty(PropertyName = "type", Required = Required.Always)]
        public string Type { get; set; }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
