using LiteServer.Controllers.Exceptions;
using LiteServer.IO.DAL.Model;
using LiteServer.IO.DAL.Repository;
using System;

namespace LiteServer.Controllers.Extensions
{
    public static class SecureExtensions
    {
        public static Token ValidateToken(this ITokenRepository tokenRepository, Guid tokenGuid)
        {
            var token = tokenRepository.TrySelect(tokenGuid);
            if (token == null)
            {
                throw new AuthorizationException("Token not found.", "invalid token");
            }

            if (token.Expired)
            {
                throw new AuthorizationException("Token is expired.", "invalid token");
            }

            return token;
        }

        public static (User user, Token token) ValidateToken(this ITokenRepository tokenRepository, IUserRepository userRepository, Guid tokenGuid)
        {
            var token = tokenRepository.TrySelect(tokenGuid);
            if (token == null)
            {
                throw new AuthorizationException("Token not found.", "invalid token");
            }

            if (token.Expired)
            {
                throw new AuthorizationException("Token is expired.", "invalid token");
            }
            
            return (userRepository.Select(token.UserUuid), token);
        }
    }
}
