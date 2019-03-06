using LiteServer.Utils;
using PetaPoco;
using System;

namespace LiteServer.IO.DAL.Model
{
    [ExplicitColumns]
    [TableName("token")]
    [PrimaryKey(nameof(ValueBinary))]
    public class Token
    {
        public bool Expired
        {
            get
            {
                return ExpireDate < DateTime.UtcNow;
            }
        }

        public Guid Value
        {
            get
            {
                if (value == null)
                {
                    value = ValueBinary.ToGuid();
                }

                return value.Value;
            }
            set
            {
                this.value = value;
                ValueBinary = value.ToBytes();
            }
        }

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

        [Column("value")]
        public byte[] ValueBinary
        { get; set; }
        [Column("expires")]
        public DateTime ExpireDate
        { get; set; }
        [Column("user_uuid")]
        public byte[] UserUuidBinary
        { get; set; }

        private Guid? value;
        private Guid? userUuid;
    }
}
