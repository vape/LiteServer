using Newtonsoft.Json;

namespace LiteServer.Models
{
    public class SessionCodeModel
    {
        [JsonProperty("value")]
        public string Value;
    }
}
