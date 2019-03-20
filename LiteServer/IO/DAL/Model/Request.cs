using LiteServer.Utils;
using PetaPoco;
using System;

namespace LiteServer.IO.DAL.Model
{
    [ExplicitColumns]
    [TableName("request")]
    [PrimaryKey("id", AutoIncrement = true)]
    public class Request
    {
        public Guid Sender
        {
            get
            {
                if (sender == null)
                {
                    sender = SenderBinary.ToGuid();
                }

                return sender.Value;
            }
            set
            {
                sender = value;
                SenderBinary = value.ToBytes();
            }
        }

        [Column("id")]
        public uint Id
        { get; set; }
        [Column("sender")]
        public byte[] SenderBinary
        { get; set; }
        [Column("type")]
        public byte Type
        { get; set; }
        [Column("amount")]
        public int Amount
        { get; set; }
        [Column("filled")]
        public int Filled
        { get; set; }

        private Guid? sender;
    }
}
