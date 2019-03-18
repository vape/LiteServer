using LiteServer.Controllers.Chats;
using LiteServer.Controllers.Exceptions;
using LiteServer.Controllers.Extensions;
using LiteServer.IO.DAL.Repository;
using LiteServer.Middleware;
using LiteServer.Models;
using LiteServer.Models.Payload;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        public const int MaxFetchLimit = 500;
        public const int DefaultFetchLimit = 20;

        private readonly ITokenRepository tokenRepository;
        private readonly IMessageRepository messageRepository;
        private readonly IUserRepository userRepository;
        private readonly IGroupRepository groupRepository;
        private readonly IChatManager chatManager;

        public MessageController(
            ITokenRepository tokenRepository, IMessageRepository messageRepository, IUserRepository userRepository, IGroupRepository groupRepository, 
            IChatManager chatManager)
        {
            this.tokenRepository = tokenRepository;
            this.messageRepository = messageRepository;
            this.userRepository = userRepository;
            this.groupRepository = groupRepository;
            this.chatManager = chatManager;
        }

        [HttpPost("online")]
        public object NotifyOnline([FromQuery] TokenQueryModel tokenInfo, [FromQuery] GroupIdentityModel groupIdentity)
        {
            var userContext = tokenRepository.ValidateToken(userRepository, tokenInfo.Token);
            if (!groupRepository.IsMember(userContext.user.Uuid, groupIdentity.GroupId))
            {
                throw new AuthorizationException($"User {userContext.user.Uuid} isn't group member", "not a group member");
            }

            chatManager.NotifyConnectedAsync(groupIdentity.GroupId, userContext.user.Uuid);

            return new OperationResultModel()
            {
                Result = true
            };
        }

        [HttpGet("fetch")]
        public object FetchMessages([FromQuery] TokenQueryModel tokenInfo, [FromQuery] GroupIdentityModel groupIdentity, [FromQuery] MessageRangeInfoModel rangeInfo)
        {
            var userContext = tokenRepository.ValidateToken(userRepository, tokenInfo.Token);
            if (!groupRepository.IsMember(userContext.user.Uuid, groupIdentity.GroupId))
            {
                throw new AuthorizationException($"User {userContext.user.Uuid} isn't group member", "not a group member");
            }

            var ascendingOrder = rangeInfo.Ascending;
            var offset = rangeInfo.Offset;
            var limit = rangeInfo.Limit > MaxFetchLimit ? MaxFetchLimit : rangeInfo.Limit == 0 ? DefaultFetchLimit : rangeInfo.Limit;
            var result = new List<MessageModel>();
            var forward = rangeInfo.Direction >= 0;

            if (rangeInfo.StartId == 0)
            {
                var range = messageRepository.SelectRange(groupIdentity.GroupId, offset, limit);
                foreach (var messageData in (ascendingOrder ? range.Reverse() : range))
                {
                    result.Add(MessageModel.Create(messageData));
                }
            }
            else
            {
                var range =
                    forward ?
                    messageRepository.SelectMessagesAfterGiven(groupIdentity.GroupId, offset, limit, rangeInfo.StartId) :
                    messageRepository.SelectMessagesBeforeGiven(groupIdentity.GroupId, offset, limit, rangeInfo.StartId);

                foreach (var messageData in (ascendingOrder ? range : range.Reverse()))
                {
                    result.Add(MessageModel.Create(messageData));
                }
            }
            
            return result;
        }

        [HttpPost("send")]
        public object SendMessage([FromQuery] TokenQueryModel tokenInfo, [FromQuery] GroupIdentityModel groupIdentity, [FromBody] MessageDataModel messageData)
        {
            var userContext = tokenRepository.ValidateToken(userRepository, tokenInfo.Token);
            if (!groupRepository.IsMember(userContext.user.Uuid, groupIdentity.GroupId))
            {
                throw new AuthorizationException($"User {userContext.user.Uuid} isn't group member", "not a group member");
            }

            messageRepository.Send(userContext.user.Uuid, groupIdentity.GroupId, messageData.Text);
            chatManager.NotifyNewMessageAsync(groupIdentity.GroupId);

            return new OperationResultModel()
            {
                Result = true
            };
        }
    }
}
