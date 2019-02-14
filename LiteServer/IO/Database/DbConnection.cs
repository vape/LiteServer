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
                "SELECT hex(user.uuid) uuid, user.name name, user.email email, social_vk.vk_id vk_id, social_vk.vk_token vk_token " + 
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

                    result.Add((
                        new User() { Uuid = userUuid, Name = name, Email = email }, 
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
                    Email = reader.GetString("email")
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
            using (var command = CreateCommand("SELECT hex(user_uuid) user_uuid, expires, hex(token_string) token_string FROM token WHERE token.token_string=unhex(@0)", tokenUuidString))
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

        public User SelectUser(Guid userUuid)
        {
            var userUuidString = userUuid.ToString("N");
            using (var command = CreateCommand("SELECT hex(uuid) uuid, name, email FROM user WHERE user.uuid=unhex(@0)", userUuidString))
            using (var reader = command.ExecuteReader())
            {
                reader.Read();

                return new User()
                {
                    Uuid = Guid.Parse(reader.GetString("uuid")),
                    Name = reader.GetString("name"),
                    Email = reader.GetString("email")
                };
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
