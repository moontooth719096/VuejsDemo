namespace DemoProgressBarAPI.Models.ChatRoom
{
    public class ChatUser
    {
        public string UserName { get; set; }
        public string UserId {  get; set; }
        public string ConnectionID {  get; set; }
        public string imgPath => "https://fakeimg.pl/60/";
        public int NoReadCount { get; set; }
        public string LastMesage { get; set; }
    }
}
