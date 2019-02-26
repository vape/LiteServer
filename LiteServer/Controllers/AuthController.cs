using LiteServer.Config;
using LiteServer.Controllers.Exceptions;
using LiteServer.IO;
using LiteServer.IO.Database;
using LiteServer.Models;
using LiteServer.Models.Query;
using LiteServer.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace LiteServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class AuthController : ControllerBase
    {
        public const int MaxNameLength = 48;
        public const int MaxEmailLength = 64;
        public const int PasswordHashIterations = 4096;
        public const int PasswordMinLength = 6;
        public const int PasswordMaxLength = 64;

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
        public object GetIsAuthenticated([FromQuery] SessionCodeQueryModel parameters)
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

        [HttpPost("authorize")]
        public object AuthorizeWithLoginAndPassword([FromQuery] AuthorizationQueryModel authData)
        {
            using (var con = new DbConnection(connectionSettings.ConnectionString))
            {
                var userData = con.SelectUserData(authData.Login);
                var givenPassword = System.Text.Encoding.UTF8.GetBytes(authData.Password);

                using (var pbkdf2 = new Rfc2898DeriveBytes(givenPassword, userData.salt, PasswordHashIterations, HashAlgorithmName.SHA256))
                {
                    var hash = pbkdf2.GetBytes(32);
                    if (Toolbox.UnsafeCompare(hash, userData.hash))
                    {
                        var token = con.CallCreateToken(userData.uuid, DateTime.UtcNow + new TimeSpan(365, 0, 0, 0));
                        return new TokenModel()
                        {
                            ExpireDate = token.ExpireDate,
                            UserUuid = token.UserUuid,
                            Value = token.Value
                        };
                    }
                }

                throw new AuthenticationException("invalid login or password");
            }
        }

        [HttpPost("register")]
        public object RegisterUser([FromQuery] RegistrationQueryModel registrationData)
        {
            using (var con = new DbConnection(connectionSettings.ConnectionString))
            {
                var password = System.Text.Encoding.UTF8.GetBytes(registrationData.Password);
                var salt = new byte[32];
                RandomNumberGenerator.Fill(salt);
                var hash = new byte[32];

                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, PasswordHashIterations, HashAlgorithmName.SHA256))
                {
                    hash = pbkdf2.GetBytes(32);
                }

                var user = default(IO.Database.Models.User);
                try
                {
                    user = con.InsertUser(registrationData.Name, registrationData.Login, hash, salt);
                }
                catch (MySqlException e)
                {
                    if (e.Number == 1062)
                    {
                        throw new BasicControllerException(e.Message, "user already exists", e);
                    }
                    else
                    {
                        throw;
                    }
                }

                return new UserModel()
                {
                    Email = user.Email,
                    Name = user.Name,
                    Uuid = user.Uuid
                };
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
