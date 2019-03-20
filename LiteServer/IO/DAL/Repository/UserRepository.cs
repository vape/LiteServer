using LiteServer.IO.DAL.Context;
using LiteServer.IO.DAL.Model;
using LiteServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteServer.IO.DAL.Repository
{
    public interface IUserRepository : IRepository<User, Guid>
    {
        IEnumerable<User> Select(IList<Guid> guids);
        IEnumerable<User> SelectAll();
        User SelectWithEmail(string email);
        User CreateUser(string name, string email, byte[] passwordHash, byte[] salt);

        SocialUser<SocialUserVK> CreateOrUpdateWithVK(int vkId, string vkToken, string newUserName, string newUserEmail);
    }

    public class UserRepository : IUserRepository
    {
        private BaseContext context;

        public UserRepository(BaseContext context)
        {
            this.context = context;
        }

        public void Delete(Guid id)
        {
            context.Db.Delete<User>(id.ToBytes());
        }

        public void Insert(User entity)
        {
            context.Db.Insert(entity);
        }

        public User Select(Guid id)
        {
            return context.Db.Single<User>("SELECT * FROM user WHERE user.uuid = @0", id.ToBytes());
        }

        public IEnumerable<User> Select(IList<Guid> guids)
        {
            foreach (var user in context.Db.Query<User>("SELECT * FROM user WHERE uuid IN(@0)", guids.Distinct().Select((g) => g.ToBytes())))
            {
                yield return user;
            }
        }

        public IEnumerable<User> SelectAll()
        {
            foreach (var user in context.Db.Query<User>("SELECT * FROM user"))
            {
                yield return user;
            }
        }

        public User SelectWithEmail(string email)
        {
            return context.Db.Single<User>("SELECT * FROM user WHERE user.email = @0", email);
        }

        public User CreateUser(string name, string email, byte[] passwordHash, byte[] salt)
        {
            var uuid = Guid.NewGuid();
            var user = new User()
            {
                Email = email,
                Name = name,
                PasswordHash = passwordHash,
                Salt = salt,
                Uuid = uuid,
                Deleted = false,
            };

            context.Db.Insert(user);
            return user;
        }

        public SocialUser<SocialUserVK> CreateOrUpdateWithVK(int vkId, string vkToken, string newUserName, string newUserEmail)
        {
            if (!context.Db.Exists<SocialUserVK>(vkId))
            {
                var user = CreateUser(newUserName, newUserEmail, null, null);
                var socialUser = new SocialUserVK()
                {
                    Id = vkId,
                    Token = vkToken,
                    UserUuid = user.Uuid
                };

                context.Db.Insert(socialUser);
                return new SocialUser<SocialUserVK>()
                {
                    User = user,
                    SocialProfile = socialUser
                };
            }
            else
            {
                context.Db.Execute("UPDATE social_vk SET vk_token=@0 WHERE vk_id=@1", vkToken, vkId);

                return context.Db.Fetch<User, SocialUserVK, SocialUser<SocialUserVK>>((u, s) =>
                {
                    return new SocialUser<SocialUserVK>()
                    {
                        User = u,
                        SocialProfile = s
                    };
                }, "SELECT user.*, social_vk.* FROM social_vk LEFT JOIN user ON social_vk.user_uuid=user.uuid WHERE vk_id=@0", vkId).First();
            }
        }
    }
}
