using System;

namespace LiteServer.IO.Database.Models
{
    public class Group
    {
        public uint Id;
        public short Type;
        public string Name;
        public Guid CreatorUuid;
        public DateTime CreationTime;
    }
}
