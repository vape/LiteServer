using LiteServer.Config;
using LiteServer.Controllers.Exceptions;
using LiteServer.IO.Database;
using LiteServer.Models;
using LiteServer.Models.Query;
using LiteServer.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        private DatabaseConfig databaseConfig;
        private PlatformConfig platformConfig;
        private IHostingEnvironment environment;

        public GroupController(IHostingEnvironment env, IOptions<DatabaseConfig> databaseConfig, IOptions<PlatformConfig> platformConfig)
        {
            this.databaseConfig = databaseConfig.Value;
            this.platformConfig = platformConfig.Value;
            this.environment = env;
        }

        [HttpGet("members")]
        public object GetMembers([FromQuery] TokenQueryModel token, [FromQuery] GroupIdentityModel groupIdentity)
        {
            using (var con = new DbConnection(databaseConfig.ConnectionString))
            {
                var tokenData = ControllerHelper.SelectAndValidateToken(con, token.Token);
                var membersData = con.SelectGroupMembers(groupIdentity.GroupId);
                var result = new List<GroupMemberModel>();

                foreach (var memberData in membersData) {
                    result.Add(new GroupMemberModel() {
                        GroupRole = memberData.Role,
                        UserName = memberData.User.Name,
                        UserUuid = memberData.User.Uuid
                    });
                }

                return result;
            }
        }

        [HttpGet("get")]
        public object GetGroup([FromQuery] GroupIdentityModel groupIdentity)
        {
            using (var con = new DbConnection(databaseConfig.ConnectionString))
            {
                var groupData = con.SelectGroupAndMembersCount(groupIdentity.GroupId);

                return new GroupModel()
                {
                    CreatorUuid = groupData.group.CreatorUuid,
                    Id = groupData.group.Id,
                    MembersCount = groupData.membersCount,
                    Name = groupData.group.Name,
                    Type = groupData.group.Type
                };
            }
        }

        [HttpPost("create")]
        public object CreateGroup([FromQuery] NewGroupInfoQueryModel groupInfo, [FromQuery] TokenQueryModel token)
        {
            if (groupInfo.Name.Length > MaxGroupNameLength)
            {
                throw new Exceptions.FormatException("Group name is too long.", "group name is too long");
            }

            if (!Enum.IsDefined(typeof(GroupType), groupInfo.Type))
            {
                throw new Exceptions.FormatException("Unknown group type.", "unknown group type");
            }

            using (var con = new DbConnection(databaseConfig.ConnectionString))
            {
                var userToken = ControllerHelper.SelectAndValidateToken(con, token.Token);
                var group = con.InsertGroup(groupInfo.Type, groupInfo.Name, userToken.UserUuid);

                return new GroupModel()
                {
                    Id = group.Id,
                    Name = group.Name,
                    Type = group.Type,
                    CreatorUuid = group.CreatorUuid,
                };
            }
        }

        [HttpPost("join")]
        public object JoinGroup([FromQuery] TokenQueryModel token, [FromQuery] GroupIdentityModel groupIdentity)
        {
            using (var con = new DbConnection(databaseConfig.ConnectionString))
            {
                var userData = ControllerHelper.SelectAndValidateTokenAndUser(con, token.Token);
                var groupData = con.SelectGroupAndMembersCount(groupIdentity.GroupId);

                if (groupData.membersCount >= platformConfig.GroupMaxMembers)
                {
                    throw new BasicControllerException("Group is full.", "group is full");
                }

                var joined = false;
                var exception = default(Exception);

                try
                {
                    joined = con.InsertGroupMember(userData.user.Uuid, groupIdentity.GroupId, (short)GroupType.Default);
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
        }

        [HttpPost("leave")]
        public object LeaveGroup([FromQuery] TokenQueryModel token, [FromQuery] GroupIdentityModel groupIdentity)
        {
            using (var con = new DbConnection(databaseConfig.ConnectionString))
            {
                var userData = ControllerHelper.SelectAndValidateTokenAndUser(con, token.Token);

                var left = false;
                var exception = default(Exception);

                try
                {
                    left = con.DeleteGroupMember(userData.user.Uuid, groupIdentity.GroupId);
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
}
