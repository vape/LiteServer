using LiteServer.Utils;
using PetaPoco;
using System;

namespace LiteServer.IO.DAL.Model
{
    [ExplicitColumns]
    [TableName("group")]
    [PrimaryKey(nameof(Id), AutoIncrement = true)]
    public class Group
    {
        public Guid CreatorUuid
        {
            get
            {
                if (creatorUuid == null)
                {
                    creatorUuid = CreatorUuidBinary.ToGuid();
                }

                return creatorUuid.Value;
            }
            set
            {
                creatorUuid = value;
                CreatorUuidBinary = value.ToBytes();
            }
        }

        [Column("id")]
        public uint Id
        { get; set; }
        [Column("type")]
        public short Type
        { get; set; }
        [Column("name")]
        public string Name
        { get; set; }
        [Column("creator_uuid")]
        public byte[] CreatorUuidBinary
        { get; set; }
        [Column("creation_time")]
        public DateTime CreationTime
        { get; set; }

        private Guid? creatorUuid;
    }
}
