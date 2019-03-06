using LiteServer.Utils;
using PetaPoco;
using System;

namespace LiteServer.IO.DAL.Model
{
    [ExplicitColumns]
    [TableName("group_member")]
    public class GroupMember
    {
        public Guid UserUuid
        {
            get
            {
                if (userUuid == null)
                {
                    userUuid = UserUuidBinary.ToGuid();
                }

                return userUuid.Value;
            }
            set
            {
                userUuid = value;
                UserUuidBinary = value.ToBytes();
            }
        }

        [Column("user_role")]
        public byte Role
        { get; set; }
        [Column("group_id")]
        public uint GroupId
        { get; set; }
        [Column("user_uuid")]
        public byte[] UserUuidBinary
        { get; set; }

        private Guid? userUuid;
    }
}
