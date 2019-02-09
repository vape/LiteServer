using Newtonsoft.Json;
using System;

namespace LiteServer.IO.Database.Models
{
    public class User
    {
        [JsonProperty("user_uuid")]
        public Guid Uuid;
    }
}
