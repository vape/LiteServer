using Newtonsoft.Json;
using System;

namespace LiteServer.Models
{
    public class UserModel
    {
        [JsonProperty("user_uuid")]
        public Guid Uuid;
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("email")]
        public string Email;
    }
}
