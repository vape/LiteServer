using LiteServer.Controllers.Exceptions;
using LiteServer.IO.Database;
using LiteServer.IO.Database.Models;
using LiteServer.Models;
using LiteServer.Models.Query;
using System;

namespace LiteServer.Utils
{
    public static class ControllerHelper
    {
        public static Token SelectAndValidateToken(DbConnection db, Guid tokenGuid)
        {
            Token token;
            if (!db.TrySelectToken(tokenGuid, out token))
            {
                throw new AuthorizationException("Token not found.", "invalid token");
            }

            if (token.IsExpired)
            {
                throw new AuthorizationException("Token is expired.", "invalid token");
            }

            return token;
        }

        public static (Token token, User user) SelectAndValidateTokenAndUser(DbConnection db, Guid tokenGuid)
        {
            Token token;
            User user;

            if (!db.TrySelectTokenAndUser(tokenGuid, out token, out user))
            {
                throw new AuthorizationException("Token not found.", "invalid token");
            }

            if (token.IsExpired)
            {
                throw new AuthorizationException("Token is expired.", "invalid token");
            }

            return (token, user);
        }
    }
}
