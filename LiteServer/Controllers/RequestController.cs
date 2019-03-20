using LiteServer.Controllers.Chats;
using LiteServer.Controllers.Exceptions;
using LiteServer.Controllers.Extensions;
using LiteServer.IO.DAL.Model;
using LiteServer.IO.DAL.Repository;
using LiteServer.Models;
using LiteServer.Models.Payload;
using Microsoft.AspNetCore.Mvc;
using System;

namespace LiteServer.Controllers
{
    public enum RequestType : byte
    {
        Unknown = 0,
        Lives
    }

    [Route("api/[controller]")]
    [ApiController]
    public class RequestController
    {
        private readonly ITokenRepository tokenRepository;
        private readonly IUserRepository userRepository;
        private readonly IGroupRepository groupRepository;
        private readonly IMessageRepository messageRepository;
        private readonly IRequestRepository requestRepository;
        private readonly IChatManager chatManager;

        public RequestController(
            ITokenRepository tokenRepository, IUserRepository userRepository, IGroupRepository groupRepository, 
            IMessageRepository messageRepository, IRequestRepository requestRepository, IChatManager chatManager)
        {
            this.tokenRepository = tokenRepository;
            this.userRepository = userRepository;
            this.groupRepository = groupRepository;
            this.messageRepository = messageRepository;
            this.requestRepository = requestRepository;
            this.chatManager = chatManager;
        }

        [HttpPost("create")]
        public object CreateRequest([FromQuery] TokenQueryModel tokenInfo, [FromQuery] GroupIdentityModel groupIdentity, [FromQuery] RequestQueryModel requestInfo)
        {
            var userContext = tokenRepository.ValidateToken(userRepository, tokenInfo.Token);
            if (!groupRepository.IsMember(userContext.user.Uuid, groupIdentity.GroupId))
            {
                throw new AuthorizationException($"User {userContext.user.Uuid} isn't group member", "not a group member");
            }

            if (requestInfo.Type == 0 || !Enum.IsDefined(typeof(RequestType), requestInfo.Type))
            {
                throw new BasicControllerException("Request type not defined.", "invalid request type", statusCode: System.Net.HttpStatusCode.UnprocessableEntity);
            }

            var type = (RequestType)requestInfo.Type;
            var request = default(Request);

            switch (type)
            {
                case RequestType.Lives:
                    request = new Request()
                    {
                        Type = (byte)RequestType.Lives,
                        Amount = 5,
                        Filled = 0,
                        Sender = userContext.user.Uuid
                    };
                    break;
                default:
                    throw new BasicControllerException($"Handler for {type} reuqest type not implemented.", "server error", statusCode: System.Net.HttpStatusCode.InternalServerError);
            }

            requestRepository.Insert(request);
            messageRepository.Send(userContext.user.Uuid, groupIdentity.GroupId, String.Empty, (byte)AttachmentType.Request, request.Id);
            chatManager.NotifyNewMessageAsync(groupIdentity.GroupId);

            return RequestModel.Create(request);
        }

        [HttpGet("get")]
        public object GetRequestData([FromQuery] TokenQueryModel tokenInfo, [FromQuery] RequestIdentityModel requestIdentity)
        {
            var userContext = tokenRepository.ValidateToken(userRepository, tokenInfo.Token);
            var request = requestRepository.Select(requestIdentity.RequestId);
            return RequestModel.Create(request);
        }

        [HttpPost("fill")]
        public object FillRequest([FromQuery] TokenQueryModel tokenInfo, [FromQuery] RequestIdentityModel requestIdentity)
        {
            var userContext = tokenRepository.ValidateToken(userRepository, tokenInfo.Token);
            if (requestRepository.FillRequest(requestIdentity.RequestId))
            {
                return RequestModel.Create(requestRepository.Select(requestIdentity.RequestId));
            }

            throw new BasicControllerException("Failed to fill request", "failed to fill request");
        }
    }
}
