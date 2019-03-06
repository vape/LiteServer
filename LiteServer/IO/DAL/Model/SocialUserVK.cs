using LiteServer.Utils;
using PetaPoco;
using System;

namespace LiteServer.IO.DAL.Model
{
    [ExplicitColumns]
    [TableName("social_vk")]
    [PrimaryKey("vk_id")]
    public class SocialUserVK
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

        [Column("user_uuid")]
        public byte[] UserUuidBinary
        { get; set; }
        [Column("vk_token")]
        public string Token
        { get; set; }
        [Column("vk_id")]
        public int Id
        { get; set; }

        private Guid? userUuid;
    }
}
