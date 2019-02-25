using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LiteServer.Models
{
    public class GroupModel
    {
        [JsonProperty("id")]
        public uint Id;
        [JsonProperty("type")]
        public short Type;
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("creator")]
        public Guid CreatorUuid;
        [JsonProperty("members_count")]
        public int MembersCount = -1;

        public bool ShouldSerializeMembersCount()
        {
            return MembersCount != -1;
        }
    }
}
