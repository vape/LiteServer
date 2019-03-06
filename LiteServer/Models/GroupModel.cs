using LiteServer.IO.DAL.Model;
using Newtonsoft.Json;
using System;

namespace LiteServer.Models
{
    public class GroupModel
    {
        public static GroupModel Create(Group group, int membersCount = -1)
        {
            return new GroupModel()
            {
                Id = group.Id,
                CreatorUuid = group.CreatorUuid,
                Name = group.Name,
                Type = group.Type,
                MembersCount = membersCount
            };
        }

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
