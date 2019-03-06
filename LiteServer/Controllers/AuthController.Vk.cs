using LiteServer.Controllers.Exceptions;
using LiteServer.IO;
using LiteServer.SocialApi;
using Microsoft.AspNetCore.Mvc;

namespace LiteServer.Controllers
{
    public partial class AuthController : ControllerBase
    {
        [HttpGet("vk/requiredScope")]
        public object GetVkRequiredScope()
        {
            return new { scope = "offline,email" };
        }

        [HttpGet("vk/redirect")]
        public object GetVkRedirectPage()
        {
            if (HttpContext.Request.Query.ContainsKey("error"))
            {
                throw new AuthenticationException($"Vk failed to authenticate user with error: {HttpContext.Request.Query["error"]}", "auth failed");
            }

            var vkCode = HttpContext.Request.Query["code"];
            var vkApi = new VkApi(socialConfig.Vk);
            var (vkAccessToken, vkUserId, vkEmail) = vkApi.RequestAccessToken(vkCode);

            if (vkEmail.Length > MaxEmailLength)
            {
                vkEmail = null;
            }

            if (vkAccessToken == null)
            {
                throw new AuthenticationException("Failed to obtain vk access token.");
            }

            // TODO: Check given permissions
            vkApi.SetToken(vkAccessToken);

            var sessionCode = HttpContext.Request.Query["state"];
            var sessionHandler = AuthSessionStorage.GetHandler(sessionCode);

            if (sessionHandler == null)
            {
                throw new AuthenticationException("Session handler not found.");
            }

            if (sessionHandler.IsExpired)
            {
                throw new AuthenticationException("Session handler has expired.");
            }

            var userInfo = vkApi.GetUserName();
            var name = FormatName(userInfo.firstName, userInfo.lastName);

            var data = userRepository.CreateOrUpdateWithVK(vkUserId, vkAccessToken, name, vkEmail);
            sessionHandler.UserUuid = data.User.Uuid;

#if DEBUG
            return data;
#else
            return new OperationResultModel() { Result = true };
#endif 
        }
    }
}
