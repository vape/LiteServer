using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LiteServer.Models
{
    public class GroupMemberModel
    {
        [JsonProperty("name")]
        public string UserName;
        [JsonProperty("uuid")]
        public Guid UserUuid;
        [JsonProperty("role")]
        public short GroupRole;
    }
}
