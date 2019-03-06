using LiteServer.Config;
using LiteServer.Controllers.Exceptions;
using LiteServer.Controllers.Extensions;
using LiteServer.IO.DAL.Repository;
using LiteServer.Models;
using LiteServer.Models.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace LiteServer.Controllers
{
    public enum GroupType : short
    {
        Default = 0
    }

    public enum GroupMemberRole : byte
    {
        Default = 0,
        Creator = 1,
    }

    [Route("api/[controller]")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        public const int MaxGroupNameLength = 24;

        private readonly IGroupRepository groupRepository;
        private readonly ITokenRepository tokenRepository;
        private readonly IUserRepository userRepository;
        private readonly PlatformConfig platformConfig;

        public GroupController(IGroupRepository groupRepository, ITokenRepository tokenRepository, IUserRepository userRepository, IOptions<PlatformConfig> platformConfig)
        {
            this.groupRepository = groupRepository;
            this.tokenRepository = tokenRepository;
            this.userRepository = userRepository;
            this.platformConfig = platformConfig.Value;
        }

        [HttpGet("members")]
        public object GetMembers([FromQuery] TokenQueryModel tokenData, [FromQuery] GroupIdentityModel groupIdentity)
        {
            var token = tokenRepository.ValidateToken(tokenData.Token);
            var members = groupRepository.SelectMembers(groupIdentity.GroupId);
            var result = new List<GroupMemberModel>();

            foreach (var member in members) {
                result.Add(GroupMemberModel.Create(member));
            }

            return result;
        }

        [HttpGet("get")]
        public object GetGroup([FromQuery] GroupIdentityModel groupIdentity)
        {
            var group = groupRepository.Select(groupIdentity.GroupId);
            var membersCount = groupRepository.SelectMembersCount(groupIdentity.GroupId);

            return GroupModel.Create(group, membersCount);
        }

        [HttpPost("create")]
        public object CreateGroup([FromQuery] NewGroupInfoQueryModel groupInfo, [FromQuery] TokenQueryModel tokenData)
        {
            if (groupInfo.Name.Length > MaxGroupNameLength)
            {
                throw new Exceptions.FormatException("Group name is too long.", "group name is too long");
            }

            if (!Enum.IsDefined(typeof(GroupType), groupInfo.Type))
            {
                throw new Exceptions.FormatException("Unknown group type.", "unknown group type");
            }

            var token = tokenRepository.ValidateToken(tokenData.Token);
            var group = groupRepository.CreateGroup(groupInfo.Name, groupInfo.Type, token.UserUuid);

            return GroupModel.Create(group);
        }

        [HttpPost("join")]
        public object JoinGroup([FromQuery] TokenQueryModel tokenData, [FromQuery] GroupIdentityModel groupIdentity)
        {
            var userContext = tokenRepository.ValidateToken(userRepository, tokenData.Token);
            var group = groupRepository.Select(groupIdentity.GroupId);
            var membersCount = groupRepository.SelectMembersCount(groupIdentity.GroupId);

            if (membersCount >= platformConfig.GroupMaxMembers)
            {
                throw new BasicControllerException("Group is full.", "group is full");
            }

            var joined = false;
            var exception = default(Exception);

            try
            {
                joined = groupRepository.InsertMember(userContext.user.Uuid, group.Id, (byte)GroupMemberRole.Default);
            }
            catch (MySqlException e)
            {
                if (e.Number == 1062)
                {
                    throw new BasicControllerException(e.Message, "already joined", e);
                }
                else
                {
                    exception = e;
                }
            }
            catch (Exception e)
            {
                exception = e;
            }
            finally
            {
                if (!joined && exception != null)
                {
                    throw new BasicControllerException("Failed to insert new group member.", "failed to join group", exception);
                }
            }

            return new OperationResultModel() { Result = joined };
        }

        [HttpPost("leave")]
        public object LeaveGroup([FromQuery] TokenQueryModel tokenData, [FromQuery] GroupIdentityModel groupIdentity)
        {
            var userContext = tokenRepository.ValidateToken(userRepository, tokenData.Token);

            var left = false;
            var exception = default(Exception);

            try
            {
                left = groupRepository.DeleteMember(userContext.user.Uuid, groupIdentity.GroupId);
            }
            catch (Exception e)
            {
                exception = e;
            }
            finally
            {
                if (!left)
                {
                    throw new BasicControllerException("Failed to leave group.", "failed to leave group", exception);
                }
            }

            return new OperationResultModel() { Result = left };
        }
    }
}
