using LiteServer.IO.DAL.Model;
using Newtonsoft.Json;
using System;

namespace LiteServer.Models
{
    public class SimpleUserModel
    {
        public static SimpleUserModel Create(User user)
        {
            return new SimpleUserModel()
            {
                Uuid = user.Uuid,
                Name = user.Name
            };
        }

        [JsonProperty("user_uuid")]
        public Guid Uuid;
        [JsonProperty("name")]
        public string Name;
    }
}
