using LiteServer.Config;
using LiteServer.IO.Database;
using LiteServer.Models;
using LiteServer.Models.Query;
using LiteServer.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LiteServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private DatabaseConfig connectionSettings;
        private SocialConfig socialConfig;

        public UserController(IOptions<DatabaseConfig> databaseConfig, IOptions<SocialConfig> socialConfig)
        {
            this.connectionSettings = databaseConfig.Value;
            this.socialConfig = socialConfig.Value;
        }

        [HttpGet("me")]
        public UserModel GetMe([FromQuery] TokenQueryModel tokenInfo)
        {
            using (var con = new DbConnection(connectionSettings.ConnectionString))
            {
                var token = ControllerHelper.ValidateToken(tokenInfo, con);
                var user = con.SelectUser(token.UserUuid);

                return new UserModel()
                {
                    Uuid = user.Uuid,
                    Name = user.Name,
                    Email = user.Email
                };
            }
        }
    }
}
