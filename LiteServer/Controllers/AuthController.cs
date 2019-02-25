using LiteServer.Config;
using LiteServer.Controllers.Exceptions;
using LiteServer.IO;
using LiteServer.IO.Database;
using LiteServer.Models;
using LiteServer.Models.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace LiteServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class AuthController : ControllerBase
    {
        public const int MaxNameLength = 48;
        public const int MaxEmailLength = 64;

        private DatabaseConfig connectionSettings;
        private SocialConfig socialConfig;

        public AuthController(IOptions<DatabaseConfig> databaseConfig, IOptions<SocialConfig> socialConfig, IOptions<PlatformConfig> platform)
        {
            this.connectionSettings = databaseConfig.Value;
            this.socialConfig = socialConfig.Value;
        }

#if DEBUG
        [HttpGet]
        public object Get()
        {
            using (var con = new DbConnection(connectionSettings.ConnectionString))
            {
                var result = new List<object>();
                foreach (var r in con.SelectAllUsers())
                {
                    result.Add(new { uuid = r.Item2.UserUuid, name = r.Item1.Name, email = r.Item1.Email, vk_id = r.Item2.VkId, vk_token = r.Item2.VkToken });
                }
                return result;
            }
        }
#endif

        [HttpGet("createSession")]
        public object GetSessionCode()
        {
            var handler = default(AuthSessionHandler);
            var result = AuthSessionStorage.CreateSessionHandler(out handler);

            if (!result)
            {
                throw new AuthenticationException("Failed to create session handler.");
            }

            return new Models.SessionCodeModel()
            {
                Value = handler.Code
            };
        }

        [HttpGet("isAuthenticated")]
        public object GetIsAuthorized([FromQuery] SessionCodeQueryModel parameters)
        {
            var handler = AuthSessionStorage.GetHandler(parameters.SessionCode);
            if (handler == null)
            {
                throw new AuthenticationException("Session handler not found.");
            }

            return new OperationResultModel()
            {
                Result = handler.UserUuid != null
            };
        }
        
        [HttpGet("createToken")]
        public object GetToken([FromQuery] SessionCodeQueryModel parameters)
        {
            var sessionHandler = AuthSessionStorage.GetHandler(parameters.SessionCode);

            if (sessionHandler == null)
            {
                throw new AuthenticationException("Session handler not found.");
            }
            else if (sessionHandler.UserUuid == null)
            {
                throw new AuthenticationException("Authentication not finished.");
            }
            else if (sessionHandler.IsExpired)
            {
                throw new AuthenticationException("Session handler has expired.");
            }

            using (var con = new DbConnection(connectionSettings.ConnectionString))
            {
                try
                {
                    var token = con.CallCreateToken(sessionHandler.UserUuid.Value, DateTime.UtcNow + new TimeSpan(365, 0, 0, 0));
                    return new TokenModel()
                    {
                        ExpireDate = token.ExpireDate,
                        UserUuid = token.UserUuid,
                        Value = token.Value
                    };
                }
                finally
                {
                    AuthSessionStorage.RemoveHandler(sessionHandler.Code);
                }
            }
        }

        private string FormatName(string firstName, string lastName)
        {
            var result =
                String.IsNullOrWhiteSpace(firstName) ?
                    (String.IsNullOrWhiteSpace(lastName) ?
                        String.Empty :
                        lastName) :
                String.IsNullOrWhiteSpace(lastName) ?
                    firstName :
                    $"{lastName} {firstName}";

            if (result.Length > MaxNameLength)
            {
                result = result.Substring(0, MaxNameLength);
            }

            return result;
        }
    }
}
