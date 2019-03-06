using LiteServer.IO.DAL.Model;
using Newtonsoft.Json;
using System;

namespace LiteServer.Models
{
    public class GroupMemberModel
    {
        public static GroupMemberModel Create(GroupMemberAndUser groupMember)
        {
            return new GroupMemberModel()
            {
                UserName = groupMember.User.Name,
                GroupRole = groupMember.Member.Role,
                UserUuid = groupMember.User.Uuid
            };
        }

        [JsonProperty("name")]
        public string UserName;
        [JsonProperty("uuid")]
        public Guid UserUuid;
        [JsonProperty("role")]
        public short GroupRole;
    }
}
