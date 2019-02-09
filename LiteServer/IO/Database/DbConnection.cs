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
        public List<(User, SocialVk)> GetAllUsers()
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT hex(user.uuid) uuid, social_vk.vk_id vk_id, social_vk.vk_token vk_token FROM user LEFT JOIN social_vk ON user.uuid=social_vk.user_uuid";

            using (var reader = command.ExecuteReader())
            {
                var result = new List<(User, SocialVk)>();
                while (reader.Read())
                {
                    var userUuid = new Guid(reader.GetString("uuid"));
                    var vkId = reader.IsDBNull(1) ? -1 : reader.GetInt32("vk_id");
                    var vkToken = reader.IsDBNull(2) ? null : reader.GetString("vk_token");

                    result.Add((new User() { Uuid = userUuid }, new SocialVk() { UserUuid = userUuid, VkId = vkId, VkToken = vkToken }));
                }

                return result;
            }
        }
#endif

        public bool GetSocialUserVk(int vkId, out SocialVk record)
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT vk_id,vk_token,hex(user_uuid) FROM social_vk WHERE vk_id=@vkid";
            command.Parameters.AddWithValue("@vkid", vkId);

            using (var reader = command.ExecuteReader())
            {
                var found = reader.Read();
                if (!found)
                {
                    record = default(SocialVk);
                    return false;
                }
                else
                {
                    var userUuid = reader.GetString("hex(user_uuid)");
                    var token = reader.GetString("vk_token");

                    record = new SocialVk()
                    {
                        UserUuid = new Guid(userUuid),
                        VkToken = token,
                        VkId = vkId
                    };

                    return true;
                }
            }
        }

        public (User user, SocialVk socialUser) CreateOrUpdateSocialUserVK(int vkId, string vkToken)
        {
            var command = connection.CreateCommand();
            command.CommandText = "CALL create_or_update_social_user_vk(@vkid, @vktoken)";
            command.Parameters.AddWithValue("@vkid", vkId);
            command.Parameters.AddWithValue("@vktoken", vkToken);

            using (var reader = command.ExecuteReader())
            {
                reader.Read();

                var userUuid = reader.GetString("user_uuid");

                var user = new User()
                {
                    Uuid = new Guid(reader.GetString("user_uuid"))
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

        public Token CreateToken(Guid userUuid, DateTime expireDate)
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
