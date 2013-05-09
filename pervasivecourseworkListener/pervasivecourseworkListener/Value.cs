using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pervasivecourseworkListener
{
    //Object-Oriented representation of an entry. the object is serializable (JSON) 
    [JsonObject]
    public class Value
    {
        [JsonProperty(PropertyName = "nodeId", Required= Required.Always)]
        public string nodeId { get; set; }

        [JsonProperty(PropertyName = "date")]
        public DateTime Stamp { get; set; }

        [JsonProperty(PropertyName = "value", Required= Required.Default, DefaultValueHandling= DefaultValueHandling.Ignore)]
        public float Val { get; set; }

        [JsonProperty(PropertyName = "type", Required = Required.Always)]
        public string Type { get; set; }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
