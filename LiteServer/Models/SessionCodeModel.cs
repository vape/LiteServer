using Newtonsoft.Json;
using System;

namespace LiteServer.Models
{
    public class SessionCodeModel
    {
        [JsonProperty("value")]
        public string Value;
    }
}
