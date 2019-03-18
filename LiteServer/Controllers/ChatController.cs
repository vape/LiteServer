using LiteServer.Controllers.Chats;
using LiteServer.Controllers.Exceptions;
using LiteServer.Controllers.Extensions;
using LiteServer.IO.DAL.Model;
using LiteServer.IO.DAL.Repository;
using LiteServer.Middleware;
using LiteServer.Models;
using LiteServer.Models.Payload;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace LiteServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ITokenRepository tokenRepository;
        private readonly IUserRepository userRepository;
        private readonly IGroupRepository groupRepository;
        private readonly IChatManager chatManager;

        public ChatController(ITokenRepository tokenRepository, IUserRepository userRepository, IGroupRepository groupRepository, IChatManager chatManager)
        {
            this.tokenRepository = tokenRepository;
            this.userRepository = userRepository;
            this.groupRepository = groupRepository;
            this.chatManager = chatManager;
        }

        [HttpGet("connect")]
        public async Task CreateSocketConnection([FromQuery] TokenQueryModel tokenInfo, [FromQuery] GroupIdentityModel groupIdentity)
        {
            var userContext = ValidateGroupMember(tokenInfo.Token, groupIdentity.GroupId);

            if (ControllerContext.HttpContext.WebSockets.IsWebSocketRequest)
            {
                var socket = await ControllerContext.HttpContext.WebSockets.AcceptWebSocketAsync();
                var ct = ControllerContext.HttpContext.RequestAborted;

                await chatManager.Listen(groupIdentity.GroupId, userContext.user.Uuid, socket, ct);
            }
        }

        [HttpGet("online")]
        public object NotifyOnline([FromQuery] TokenQueryModel tokenInfo, [FromQuery] GroupIdentityModel groupIdentity)
        {
            var userContext = ValidateGroupMember(tokenInfo.Token, groupIdentity.GroupId);

            var count = chatManager.GetConnectUsersCount(groupIdentity.GroupId);

            return new OperationResultModel<int>()
            {
                Result = count
            };
        }

        private (User user, Token token) ValidateGroupMember(Guid token, uint groupId)
        {
            var userContext = tokenRepository.ValidateToken(userRepository, token);
            if (!groupRepository.IsMember(userContext.user.Uuid, groupId))
            {
                throw new AuthorizationException($"User {userContext.user.Uuid} isn't group member", "not a group member");
            }

            return userContext;
        }
    }
}