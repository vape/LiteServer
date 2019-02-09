using LiteServer.Config;
using LiteServer.Controllers.Exceptions;
using LiteServer.IO;
using LiteServer.IO.Database;
using LiteServer.Models;
using LiteServer.Models.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace LiteServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class AuthController : ControllerBase
    {
        private DatabaseConfig connectionSettings;
        private SocialConfig socialConfig;

        public AuthController(IOptions<DatabaseConfig> databaseConfig, IOptions<SocialConfig> socialConfig)
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
                foreach (var r in con.GetAllUsers())
                {
                    result.Add(new { uuid = r.Item2.UserUuid, vk_id = r.Item2.VkId, vk_token = r.Item2.VkToken });
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
                throw new AuthException("Failed to create session handler.");
            }

            return new SessionCodeModel()
            {
                Value = handler.Code
            };
        }

        [HttpGet("redirect/vk")]
        public object GetVkRedirectPage()
        {
            if (HttpContext.Request.Query.ContainsKey("error"))
            {
                throw new AuthException("Vk returns error on redirect.");
            }

            var vkCode = HttpContext.Request.Query["code"];
            var (vkAccessToken, vkUserId) = RequestVkAccessToken(vkCode);

            if (vkAccessToken == null)
            {
                throw new AuthException("Failed to get access token.");
            }

            var sessionCode = HttpContext.Request.Query["state"];
            var sessionHandler = AuthSessionStorage.GetHandler(sessionCode);

            if (sessionHandler == null)
            {
                throw new AuthException("Failed to find proper auth session handler.");
            }

            if (sessionHandler.IsExpired)
            {
                throw new AuthException("Auth session expired.");
            }

            using (var con = new DbConnection(connectionSettings.ConnectionString))
            {
                var data = con.CreateOrUpdateSocialUserVK(vkUserId, vkAccessToken);
                sessionHandler.UserUuid = data.user.Uuid;
                return data;
            }
        }

        [HttpGet("isAuthorized")]
        public object GetIsAuthorized([FromQuery] SessionCodeQueryModel parameters)
        {
            var handler = AuthSessionStorage.GetHandler(parameters.SessionCode);
            if (handler == null)
            {
                return new AuthException("Failed to find session handler.");
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
                throw new AuthException("Session handler not found.");
            }
            else if (sessionHandler.UserUuid == null)
            {
                throw new AuthException("Authorization not complete.");
            }
            else if (sessionHandler.IsExpired)
            {
                throw new AuthException("Session handler expired.");
            }

            using (var con = new DbConnection(connectionSettings.ConnectionString))
            {
                try
                {
                    var token = con.CreateToken(sessionHandler.UserUuid.Value, DateTime.UtcNow + new TimeSpan(365, 0, 0, 0));
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

        private (string, int) RequestVkAccessToken(string vkCode)
        {
            try
            {
                var uri =
                    $"https://oauth.vk.com/access_token?" +
                    $"client_id={socialConfig.Vk.AppId}&client_secret={socialConfig.Vk.SecureKey}&" +
                    $"redirect_uri={socialConfig.Vk.RedirectUri}&code={vkCode}";

                var request = WebRequest.Create(uri);
                var response = request.GetResponse();

                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    var json = JObject.Parse(reader.ReadToEnd());
                    var token = json["access_token"].ToString();
                    var userid = int.Parse(json["user_id"].ToString());

                    return (token, userid);
                }
            }
            catch (Exception e)
            {
                return (null, 0);
            }
        }
    }
}
