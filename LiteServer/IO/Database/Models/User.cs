using Newtonsoft.Json;
using System;

namespace LiteServer.IO.Database.Models
{
    public class User
    {
        public Guid Uuid;
        public string Name;
        public string Email;
        public bool Deleted;
    }
}
