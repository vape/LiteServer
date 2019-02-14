using LiteServer.Controllers.Exceptions;
using LiteServer.IO.Database;
using LiteServer.Models;
using LiteServer.Models.Query;

namespace LiteServer.Utils
{
    public static class ControllerHelper
    {
        public static TokenModel ValidateToken(TokenQueryModel tokenInfo, DbConnection connection)
        {
            var token = connection.SelectToken(tokenInfo.Token);
            if (token.IsExpired)
            {
                throw new AuthorizationException("Token is expired.", "invalid token");
            }

            return new TokenModel()
            {
                ExpireDate = token.ExpireDate,
                UserUuid = token.UserUuid,
                Value = token.Value
            };
        }
    }
}
