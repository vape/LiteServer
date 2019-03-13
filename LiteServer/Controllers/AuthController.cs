using LiteServer.Config;
using LiteServer.Controllers.Exceptions;
using LiteServer.IO;
using LiteServer.IO.DAL.Repository;
using LiteServer.Models;
using LiteServer.Models.Query;
using LiteServer.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

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
        public const int TokenDurationDays = 30;
        
        private readonly SocialConfig socialConfig;
        private readonly IUserRepository userRepository;
        private readonly ITokenRepository tokenRepository;

        public AuthController(IUserRepository userRepository, ITokenRepository tokenRepository, IOptions<SocialConfig> socialConfig)
        {
            this.socialConfig = socialConfig.Value;
            this.userRepository = userRepository;
            this.tokenRepository = tokenRepository;
        }

#if DEBUG
        [HttpGet]
        public object Get()
        {
            var result = new List<object>();
            foreach (var r in userRepository.SelectAll())
            { result.Add(r); }
            return result;
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

            return new SessionCodeModel()
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

            try
            {
                var token = tokenRepository.CreateToken(sessionHandler.UserUuid.Value, new TimeSpan(TokenDurationDays, 0, 0, 0));
                return TokenModel.Create(token);
            }
            finally
            {
                AuthSessionStorage.RemoveHandler(sessionHandler.Code);
            }
        }

        [HttpPost("authorize")]
        public object AuthorizeWithLoginAndPassword([FromQuery] AuthorizationQueryModel authData)
        {
            var user = userRepository.SelectWithEmail(authData.Login);
            var password = System.Text.Encoding.UTF8.GetBytes(authData.Password);

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, user.Salt, PasswordHashIterations, HashAlgorithmName.SHA256))
            {
                var hash = pbkdf2.GetBytes(32);
                if (Toolbox.UnsafeCompare(hash, user.PasswordHash))
                {
                    var token = tokenRepository.CreateToken(user.Uuid, new TimeSpan(TokenDurationDays, 0, 0, 0));
                    return TokenModel.Create(token);
                }
            }

            throw new AuthenticationException("invalid login or password");
        }

        [HttpPost("register")]
        public object RegisterUser([FromQuery] RegistrationQueryModel registrationData)
        {
            var password = Encoding.UTF8.GetBytes(registrationData.Password);
            var salt = new byte[32]; RandomNumberGenerator.Fill(salt);
            var hash = new byte[32];

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, PasswordHashIterations, HashAlgorithmName.SHA256))
            {
                hash = pbkdf2.GetBytes(32);
            }

            try
            {
                var user = userRepository.CreateUser(registrationData.Name, registrationData.Login, hash, salt);
                return UserModel.Create(user);
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
