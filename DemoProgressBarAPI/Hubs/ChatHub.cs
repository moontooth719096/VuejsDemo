using DemoProgressBarAPI.Models.ChatRoom;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DemoProgressBarAPI.Hubs
{
    //[Authorize]
    public class ChatHub : Hub
    {
        //private static List<string> _connectlist;
        private static List<ChatUser> _connectlist = new List<ChatUser>();

        public override async Task OnConnectedAsync()
        {
           
            ChatUser nowUseringfo = UserInfoGet();

            if (nowUseringfo!=null && !(_connectlist.Any(x => x.UserId == nowUseringfo.UserId)))
            {
                _connectlist.Add(nowUseringfo);
                //await Clients.All.SendAsync("UserConnected", nowUseringfo);
                await Clients.All.SendAsync("RefreshConnectList", _connectlist);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            string nowUserid = Context.ConnectionId;
            ChatUser? user = _connectlist.SingleOrDefault(x => x.ConnectionID == nowUserid);

            if (user != null)
            {
                _connectlist.Remove(user);
                await Clients.All.SendAsync("UserDisconnected", user.UserId);
            }


            await base.OnDisconnectedAsync(ex);
        }
        public async Task WorldMessage(string connectid,string message)
        {
            await Clients.All.SendAsync("WorldMessage", connectid, message);
        }

        public async Task PrivateMessage(string sendid, string message)
        {
            string nowUserid = Context.ConnectionId;
            ChatUser? user = _connectlist.SingleOrDefault(x=>x.ConnectionID == nowUserid);
            ChatUser? senduser = _connectlist.SingleOrDefault(x=>x.UserId == sendid);
            if(senduser !=null)
                await Clients.Client(senduser.ConnectionID).SendAsync("PrivateMessage", user, message);
        }

        public async Task RefreshConnectList(string nowUserid = "") 
        {
            if (!string.IsNullOrEmpty(nowUserid))
            {
                await Clients.Client(nowUserid).SendAsync("RefreshConnectList", _connectlist);
            }
            else 
            {
                await Clients.All.SendAsync("RefreshConnectList", _connectlist);
            }
            
        }

        //取得使用者資訊
        private ChatUser UserInfoGet() 
        {
            ChatUser user = null;
            //取得目前連線id
            string signalRconnectid = Context.ConnectionId;
            var httpContext = Context.GetHttpContext();
            //取得JWT內使用者資訊
            var userIdClaim = httpContext.User.Claims;
            //var claims = ((ClaimsIdentity)Context.User.Identity).Claims;
            if (userIdClaim != null && userIdClaim.Count()>0)
            {
                user = new ChatUser
                {
                    UserName = userIdClaim!?.FirstOrDefault(c => c.Type == "UserName") != null ? userIdClaim.FirstOrDefault(c => c.Type == "UserName").Value.ToString() : "",
                    UserId = userIdClaim.FirstOrDefault(c => c.Type == "UserID") != null ? userIdClaim.FirstOrDefault(c => c.Type == "UserID").Value.ToString() : "",
                    imgPath = userIdClaim.FirstOrDefault(c => c.Type == "PicturesPath") != null ? userIdClaim.FirstOrDefault(c => c.Type == "PicturesPath").Value.ToString() : "https://fakeimg.pl/80/",
                    ConnectionID = signalRconnectid
                };
            }

            return user;
        }
    }
}
