using LiteServer.IO.Database.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace LiteServer.IO.Database
{
    // TODO: There's much better architecture for DAL
    // Check out https://github.com/davybrion/companysite-dotnet/blob/master/content/blog/2009-08-build-your-own-data-access-layer-series.md and try it maybe
    public partial class DbConnection : IDisposable
    {
        private MySqlConnection connection;

        public DbConnection(string connectionString)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            connection = new MySqlConnection(connectionString);
            connection.Open();
        }

#if DEBUG
        public List<(User, SocialVk)> SelectAllUsers()
        {
            var command = connection.CreateCommand();
            command.CommandText = 
                "SELECT hex(user.uuid) uuid, user.name name, user.email email, social_vk.vk_id vk_id, social_vk.vk_token vk_token, deleted " + 
                "FROM user LEFT JOIN social_vk ON user.uuid=social_vk.user_uuid";

            using (var reader = command.ExecuteReader())
            {
                var result = new List<(User, SocialVk)>();
                while (reader.Read())
                {
                    var userUuid = new Guid(reader.GetString("uuid"));
                    var vkId = reader.IsDBNull(1) ? -1 : reader.GetInt32("vk_id");
                    var vkToken = reader.IsDBNull(2) ? null : reader.GetString("vk_token");
                    var name = reader.GetString("name");
                    var email = reader.GetString("email");
                    var deleted = reader.GetBoolean("deleted");

                    result.Add((
                        new User() { Uuid = userUuid, Name = name, Email = email, Deleted = deleted }, 
                        new SocialVk() { UserUuid = userUuid, VkId = vkId, VkToken = vkToken }
                    ));
                }

                return result;
            }
        }
#endif

        public SocialVk SelectSocialUserVk(int vkId)
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT vk_id,vk_token,hex(user_uuid) FROM social_vk WHERE vk_id=@vkid";
            command.Parameters.AddWithValue("@vkid", vkId);

            using (var reader = command.ExecuteReader())
            {
                var found = reader.Read();
                if (!found)
                {
                    return null;
                }
                else
                {
                    var userUuid = reader.GetString("hex(user_uuid)");
                    var token = reader.GetString("vk_token");

                    return new SocialVk()
                    {
                        UserUuid = new Guid(userUuid),
                        VkToken = token,
                        VkId = vkId
                    };
                }
            }
        }

        public (User user, SocialVk socialUser) CallCreateOrUpdateSocialUserVK(int vkId, string vkToken, string newUserName, string email)
        {
            var command = connection.CreateCommand();
            command.CommandText = "CALL create_or_update_social_user_vk(@vkid, @vktoken, @newusername, @email)";
            command.Parameters.AddWithValue("@vkid", vkId);
            command.Parameters.AddWithValue("@vktoken", vkToken);
            command.Parameters.AddWithValue("@newusername", newUserName);
            command.Parameters.AddWithValue("@email", email);

            using (var reader = command.ExecuteReader())
            {
                reader.Read();

                var userUuid = reader.GetString("user_uuid");

                var user = new User()
                {
                    Uuid = new Guid(reader.GetString("user_uuid")),
                    Name = reader.GetString("name"),
                    Email = reader.GetString("email"),
                    Deleted = reader.GetBoolean("deleted")
                };

                var socialUser = new SocialVk()
                {
                    UserUuid = user.Uuid,
                    VkId = vkId,
                    VkToken = vkToken
                };

                return (user, socialUser);
            }
        }

        public Token CallCreateToken(Guid userUuid, DateTime expireDate)
        {
            var command = connection.CreateCommand();
            command.CommandText = "CALL create_token(@uuid, @date)";
            command.Parameters.AddWithValue("@uuid", userUuid.ToString("N"));
            command.Parameters.AddWithValue("@date", expireDate);

            using (var reader = command.ExecuteReader())
            {
                reader.Read();

                var tokenValue = reader.GetString("token_value");
                return new Token()
                {
                    ExpireDate = expireDate,
                    UserUuid = userUuid,
                    Value = new Guid(tokenValue)
                };
            }
        }

        public Token SelectToken(Guid tokenUuid)
        {
            var tokenUuidString = tokenUuid.ToString("N");
            using (var command = CreateCommand("SELECT hex(user_uuid) user_uuid, expires, hex(value) token_string FROM token WHERE token.value=unhex(@0)", tokenUuidString))
            using (var reader = command.ExecuteReader())
            {
                reader.Read();

                return new Token()
                {
                    ExpireDate = reader.GetDateTime("expires"),
                    UserUuid = Guid.Parse(reader.GetString("user_uuid")),
                    Value = Guid.Parse(reader.GetString("token_string"))
                };
            }
        }

        public bool TrySelectToken(Guid tokenUuid, out Token token)
        {
            var tokenUuidString = tokenUuid.ToString("N");
            using (var command = CreateCommand("SELECT hex(user_uuid) user_uuid, expires, hex(value) token_string FROM token WHERE token.value=unhex(@0)", tokenUuidString))
            using (var reader = command.ExecuteReader())
            {
                if (!reader.Read())
                {
                    token = null;
                    return false;
                }

                token = new Token()
                {
                    ExpireDate = reader.GetDateTime("expires"),
                    UserUuid = Guid.Parse(reader.GetString("user_uuid")),
                    Value = Guid.Parse(reader.GetString("token_string"))
                };
                return true;
            }
        }

        public bool TrySelectTokenAndUser(Guid tokenUuid, out Token token, out User user)
        {
            var tokenUuidString = tokenUuid.ToString("N");
            using (var command = CreateCommand("CALL select_token_join_user(unhex(@0))", tokenUuidString))
            using (var reader = command.ExecuteReader())
            {
                if (!reader.Read())
                {
                    token = null;
                    user = null;
                    return false;
                }

                token = new Token()
                {
                    ExpireDate = reader.GetDateTime("expires"),
                    UserUuid = Guid.Parse(reader.GetString("user_uuid_string")),
                    Value = Guid.Parse(reader.GetString("token_string"))
                };

                user = new User()
                {
                    Uuid = Guid.Parse(reader.GetString("user_uuid_string")),
                    Name = reader.GetString("name"),
                    Email = reader.GetString("email"),
                    Deleted = reader.GetBoolean("deleted")
                };

                return true;
            }
        }

        public User SelectUser(Guid userUuid)
        {
            var userUuidString = userUuid.ToString("N");
            using (var command = CreateCommand("SELECT hex(uuid) uuid, name, email, deleted FROM user WHERE user.uuid=unhex(@0)", userUuidString))
            using (var reader = command.ExecuteReader())
            {
                reader.Read();

                return new User()
                {
                    Uuid = Guid.Parse(reader.GetString("uuid")),
                    Name = reader.GetString("name"),
                    Email = reader.GetString("email"),
                    Deleted = reader.GetBoolean("deleted")
                };
            }
        }

        public Group InsertGroup(short type, string name, Guid creator)
        {
            var creatorUuidString = creator.ToString("N");
            using (var command = CreateCommand("CALL create_group(@0, @1, @2)", name, type, creatorUuidString))
            using (var reader = command.ExecuteReader())
            {
                reader.Read();

                return new Group()
                {
                    Id = reader.GetUInt32("id"),
                    Type = reader.GetInt16("type"),
                    Name = reader.GetString("name"),
                    CreatorUuid = Guid.Parse(reader.GetString("creator_uuid_text")),
                    CreationTime = reader.GetDateTime("creation_time")
                };
            }
        }

        public Group SelectGroup(uint id)
        {
            using (var command = CreateCommand("SELECT *, hex(creator_uuid) creator_uuid_text FROM `group` WHERE id=@0", id))
            using (var reader = command.ExecuteReader())
            {
                reader.Read();

                return new Group()
                {
                    Id = reader.GetUInt32("id"),
                    Type = reader.GetInt16("type"),
                    Name = reader.GetString("name"),
                    CreatorUuid = Guid.Parse(reader.GetString("creator_uuid_text")),
                    CreationTime = reader.GetDateTime("creation_time")
                };
            }
        }

        public (Group group, int membersCount) SelectGroupAndMembersCount(uint id)
        {
            using (var command = CreateCommand("CALL select_group_and_members_count(@0)", id))
            using (var reader = command.ExecuteReader())
            {
                reader.Read();

                var group = new Group()
                {
                    Id = reader.GetUInt32("id"),
                    Type = reader.GetInt16("type"),
                    Name = reader.GetString("name"),
                    CreatorUuid = Guid.Parse(reader.GetString("creator_uuid_text")),
                    CreationTime = reader.GetDateTime("creation_time")
                };

                return (group, reader.GetInt32("members_count"));
            }
        }

        public List<GroupMember> SelectGroupMembers(uint id)
        {
            using (var command = CreateCommand("CALL get_group_members(@0)", id))
            using (var reader = command.ExecuteReader())
            {
                var result = new List<GroupMember>();
                while (reader.Read())
                {
                    result.Add(new GroupMember() {
                       Role = reader.GetByte("group_role"),
                       User = new User()
                       {
                           Name = reader.GetString("user_name"),
                           Email = reader.GetString("user_email"),
                           Uuid = Guid.Parse(reader.GetString("user_uuid"))
                       }
                    });
                }

                return result;
            }
        }

        public bool InsertGroupMember(Guid userUuid, uint groupId, short role)
        {
            var userUuidString = userUuid.ToString("N");
            using (var command = CreateCommand("CALL insert_group_member(@0, unhex(@1), @2)", groupId, userUuidString, role))
            {
                return command.ExecuteNonQuery() != 0;
            }
        }

        public bool DeleteGroupMember(Guid userUuid, uint groupId)
        {
            var userUuidString = userUuid.ToString("N");
            using (var command = CreateCommand("DELETE FROM group_member WHERE group_id=@0 AND user_uuid=unhex(@1)", groupId, userUuidString))
            {
                return command.ExecuteNonQuery() != 0;
            }
        }

        private MySqlCommand CreateCommand(string text, params (string key, object value)[] parameters)
        {
            var command = connection.CreateCommand();
            command.CommandText = text;

            for (int i = 0; i < parameters.Length; ++i)
            {
                command.Parameters.AddWithValue(parameters[i].key, parameters[i].value);
            }

            return command;
        }

        private MySqlCommand CreateCommand(string text, params object[] format)
        {
            var command = connection.CreateCommand();
            command.CommandText = text;

            for (int i = 0; i < format.Length; ++i)
            {
                command.Parameters.AddWithValue($"@{i}", format[i]);
            }

            return command;
        }

        public void Dispose()
        {
            if (connection != null)
            {
                connection.Close();
                connection = null;
            }
        }
    }
}
