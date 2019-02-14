namespace LiteServer.Config
{
    public class VkConfig
    {
        public string AppId
        { get; set; }
        public string SecureKey
        { get; set; }
        public string RedirectUri
        { get; set; }
    }

    public class SocialConfig
    {
        public VkConfig Vk
        { get; set; }
    }
}
