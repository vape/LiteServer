using LiteServer.IO.DAL.Model;
using Newtonsoft.Json;
using System;

namespace LiteServer.Models
{
    public class UserModel
    {
        public static UserModel Create(User user)
        {
            return new UserModel()
            {
                Uuid = user.Uuid,
                Name = user.Name,
                Email = user.Email
            };
        }

        [JsonProperty("user_uuid")]
        public Guid Uuid;
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("email")]
        public string Email;
    }
}
