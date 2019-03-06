using LiteServer.Utils;
using PetaPoco;
using System;

namespace LiteServer.IO.DAL.Model
{
    [ExplicitColumns]
    [TableName("user")]
    [PrimaryKey(nameof(UuidBinary))]
    public class User
    {
        public Guid Uuid
        {
            get
            {
                if (uuid == null)
                {
                    uuid = UuidBinary.ToGuid();
                }

                return uuid.Value;
            }
            set
            {
                uuid = value;
                UuidBinary = value.ToBytes();
            }
        }

        [Column("uuid")]
        public byte[] UuidBinary
        { get; set; }
        [Column("name")]
        public string Name
        { get; set; }
        [Column("email")]
        public string Email
        { get; set; }
        [Column("deleted")]
        public bool Deleted
        { get; set; }
        [Column("password")]
        public byte[] PasswordHash
        { get; set; }
        [Column("salt")]
        public byte[] Salt
        { get; set; }

        private Guid? uuid;
    }
}
