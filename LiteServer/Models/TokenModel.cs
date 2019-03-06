using LiteServer.IO.DAL.Model;
using Newtonsoft.Json;
using System;

namespace LiteServer.Models
{
    public class TokenModel
    {
        public static TokenModel Create(Token token)
        {
            return new TokenModel()
            {
                ExpireDate = token.ExpireDate,
                UserUuid = token.UserUuid,
                Value = token.Value
            };
        }

        [JsonProperty("value")]
        public Guid Value;
        [JsonProperty("user_uuid")]
        public Guid UserUuid;
        [JsonProperty("expire_date")]
        public DateTime ExpireDate;
    }
}
