using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace LiteServer.IO
{
    public class AuthSessionHandler
    {
        public bool IsExpired
        {
            get
            {
                return ExpireDate < DateTime.UtcNow;
            }
        }

        public string Code;
        public DateTime ExpireDate;
        public Guid? UserUuid;
    }

    public class AuthSessionStorage
    {
        private static ConcurrentDictionary<string, AuthSessionHandler> storage = new ConcurrentDictionary<string, AuthSessionHandler>();

        public static bool CreateSessionHandler(out AuthSessionHandler handler)
        {
            handler = new AuthSessionHandler()
            {
                Code = Guid.NewGuid().ToString("N"),
                ExpireDate = DateTime.UtcNow + new TimeSpan(1, 0, 0),
            };

            return storage.TryAdd(handler.Code, handler);
        }

        public static bool HandlerExists(string code)
        {
            return storage.ContainsKey(code);
        }

        public static AuthSessionHandler GetHandler(string code)
        {
            if (storage.ContainsKey(code))
            {
                return storage[code];
            }
            else
            {
                return null;
            }
        }

        public static bool RemoveHandler(string code)
        {
            AuthSessionHandler handler;
            return storage.Remove(code, out handler);
        }
    }
}
