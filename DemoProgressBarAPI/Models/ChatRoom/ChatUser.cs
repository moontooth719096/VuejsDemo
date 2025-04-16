using DemoProgressBarAPI.Models.User;

namespace DemoProgressBarAPI.Models.ChatRoom
{
    public class ChatUser: UserInfo
    {
        public int NoReadCount { get; set; }
        public string LastMesage { get; set; }
        public ChatUser()
        {
        }
        public ChatUser(UserInfo userinfo)
        {
            UserID = userinfo.UserID;
            UserName = userinfo.UserName;
            PicturesPath = userinfo.PicturesPath;
            UserLevel = userinfo.UserLevel;
        }
    }
}
