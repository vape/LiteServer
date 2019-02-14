using LiteServer.Config;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;

namespace LiteServer.SocialApi
{
    public class VkApi
    {
        private VkConfig config;
        private string token;
        private string version;

        public VkApi(VkConfig config, string token = null)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            this.config = config;
            this.version = "5.92";

            SetToken(token);
        }

        public void SetToken(string token)
        {
            this.token = token;
        }

        public (string token, int userId, string email) RequestAccessToken(string vkCode)
        {
            var uri =
                $"https://oauth.vk.com/access_token?" +
                $"client_id={config.AppId}&client_secret={config.SecureKey}&" +
                $"redirect_uri={config.RedirectUri}&code={vkCode}";

            var json = Request(uri);
            var token = json["access_token"].ToString();
            var userid = int.Parse(json["user_id"].ToString());
            var email = json["email"]?.ToString() ?? String.Empty;

            return (token, userid, email);
        }

        public (string firstName, string lastName) GetUserName()
        {
            CheckToken();

            var uri = $"https://api.vk.com/method/users.get?access_token={token}&v={version}";

            var json = Request(uri);
            var data = json["response"][0];
            var first = data["first_name"].ToString();
            var last = data["last_name"].ToString();

            return (first, last);
        }

        private JObject Request(string uri)
        {
            var request = WebRequest.Create(uri);
            var response = request.GetResponse();

            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                return JObject.Parse(reader.ReadToEnd());
            }
        }

        private void CheckToken()
        {
            if (this.token == null)
            {
                throw new Exception("Token is not set.");
            }
        }
    }
}
