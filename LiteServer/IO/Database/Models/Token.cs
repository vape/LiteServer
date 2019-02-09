using System;

namespace LiteServer.IO.Database.Models
{
    public class Token
    {
        public Guid Value;
        public DateTime ExpireDate;
        public Guid UserUuid;
    }
}
