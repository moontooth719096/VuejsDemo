using DemoProgressBarAPI.Models.User;

namespace DemoProgressBarAPI.Models.ChatRoom
{
    public class ChatUser: UserInfo
    {
        //public string UserName { get; set; }
        //public string UserId {  get; set; }
        public string ConnectionID { get; set; }
        //public string imgPath { get; set; }
        public int NoReadCount { get; set; }
        public string LastMesage { get; set; }
    }
}
