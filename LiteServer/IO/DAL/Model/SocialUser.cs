namespace LiteServer.IO.DAL.Model
{
    public class SocialUser<TSocialProfile> 
        where TSocialProfile: SocialUserVK
    {
        public User User
        { get; set; }
        public TSocialProfile SocialProfile
        { get; set; }
    }
}
