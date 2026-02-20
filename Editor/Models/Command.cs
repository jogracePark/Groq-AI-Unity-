using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SweetHome.Editor.Models
{
    /// <summary>
    /// Represents a generic command received from an external source.
    /// </summary>
    public class Command
    {
        [JsonProperty("type")]
        public string type;

        [JsonProperty("params")]
        public JObject @params;
    }
}
