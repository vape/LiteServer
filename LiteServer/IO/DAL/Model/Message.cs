using LiteServer.Utils;
using PetaPoco;
using System;

namespace LiteServer.IO.DAL.Model
{
    [ExplicitColumns]
    [TableName("messages")]
    [PrimaryKey("id", AutoIncrement = true)]
    public class Message
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

        [Column("id")]
        public long Id
        { get; set; }
        [Column("text")]
        public string Text
        { get; set; }
        [Column("user_uuid")]
        public byte[] UserUuidBinary
        { get; set; }
        [Column("group_id")]
        public uint GroupId
        { get; set; }
        [Column("date")]
        public DateTime Date
        { get; set; }
        [Column("att_type")]
        public byte AttachmentType
        { get; set; }
        [Column("att_ref")]
        public long AttachmentReference
        { get; set; }

        private Guid? userUuid;
    }
}
