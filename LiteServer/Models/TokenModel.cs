using Newtonsoft.Json;
using System;

namespace LiteServer.Models
{
    public class TokenModel
    {
        [JsonProperty("value")]
        public Guid Value;
        [JsonProperty("user_uuid")]
        public Guid UserUuid;
        [JsonProperty("expire_date")]
        public DateTime ExpireDate;
    }
}
