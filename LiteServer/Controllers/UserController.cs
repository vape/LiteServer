using LiteServer.Controllers.Extensions;
using LiteServer.IO.DAL.Repository;
using LiteServer.Middleware;
using LiteServer.Models;
using LiteServer.Models.Payload;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

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

        [HttpPost("info")]
        public object GetUsersInfo([FromQuery] TokenQueryModel tokenInfo, [FromBody] UsersListModel usersList)
        {
            var result = new List<SimpleUserModel>();

            foreach (var user in userRepository.Select(usersList.Users))
            {
                result.Add(SimpleUserModel.Create(user));
            }

            return result;
        }
    }
}
