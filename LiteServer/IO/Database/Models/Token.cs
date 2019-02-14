using System;

namespace LiteServer.IO.Database.Models
{
    public class Token
    {
        public bool IsExpired
        {
            get
            {
                return ExpireDate < DateTime.UtcNow;
            }
        }

        public Guid Value;
        public DateTime ExpireDate;
        public Guid UserUuid;
    }
}
