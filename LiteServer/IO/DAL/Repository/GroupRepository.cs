using LiteServer.IO.DAL.Context;
using LiteServer.IO.DAL.Model;
using LiteServer.Utils;
using System;
using System.Collections.Generic;

namespace LiteServer.IO.DAL.Repository
{
    public interface IGroupRepository : IRepository<Group, uint>
    {
        Group CreateGroup(string name, short type, Guid creator);
        List<GroupMemberAndUser> SelectMembers(uint id);
        int SelectMembersCount(uint id);
        bool InsertMember(Guid userUuid, uint groupId, byte role);
        bool DeleteMember(Guid userUuid, uint groupId);
    }

    public class GroupRepository : IGroupRepository
    {
        private BaseContext context;

        public GroupRepository(BaseContext context)
        {
            this.context = context;
        }

        public void Delete(uint id)
        {
            context.Db.Delete<Group>(id);
        }

        public void Insert(Group entity)
        {
            context.Db.Insert(entity);
        }

        public Group Select(uint id)
        {
            return context.Db.Single<Group>(id);
        }

        public int SelectMembersCount(uint id)
        {
            return context.Db.ExecuteScalar<int>("SELECT COUNT(*) FROM group_member WHERE group_member.group_id = @0", id);
        }

        public List<GroupMemberAndUser> SelectMembers(uint id)
        {
            return context.Db.Fetch<GroupMember, User, GroupMemberAndUser>((groupMember, user) => {
                return new GroupMemberAndUser()
                {
                    Member = groupMember,
                    User = user
                };
            }, "SELECT group_member.*, user.* FROM group_member LEFT JOIN user ON user.uuid = group_member.user_uuid WHERE group_member.group_id = @0", id);
        }

        public Group CreateGroup(string name, short type, Guid creator)
        {
            var group = new Group()
            {
                CreationTime = DateTime.UtcNow,
                CreatorUuid = creator,
                Name = name,
                Type = type
            };

            var result = context.Db.Insert(group);
            return group;
        }

        public bool InsertMember(Guid userUuid, uint groupId, byte role)
        {
            var member = new GroupMember()
            {
                GroupId = groupId,
                Role = role,
                UserUuid = userUuid
            };

            context.Db.Insert(member);
            return true;
        }

        public bool DeleteMember(Guid userUuid, uint groupId)
        {
            return context.Db.Execute("DELETE FROM group_member WHERE user_uuid = @0 AND group_id = @1", userUuid.ToBytes(), groupId) != 0;
        }
    }
}
