using LiteServer.Controllers.Extensions;
using LiteServer.IO.DAL.Repository;
using LiteServer.Models;
using LiteServer.Models.Query;
using Microsoft.AspNetCore.Mvc;

namespace LiteServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository userRepository;
        private readonly ITokenRepository tokenRepository;

        public UserController(IUserRepository userRepository, ITokenRepository tokenRepository)
        {
            this.userRepository = userRepository;
            this.tokenRepository = tokenRepository;
        }

        [HttpGet("me")]
        public UserModel GetMe([FromQuery] TokenQueryModel tokenInfo)
        {
            var token = tokenRepository.ValidateToken(tokenInfo.Token);
            var user = userRepository.Select(token.UserUuid);

            return UserModel.Create(user);
        }
    }
}
