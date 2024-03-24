using DemoProgressBarAPI.Models.ChatRoom;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Security.Claims;

namespace DemoProgressBarAPI.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        //private static List<string> _connectlist;
        private static List<ChatUser> _connectlist = new List<ChatUser>();

        public IEnumerable<ChatUser> ConnectListGet() 
        {
            ChatUser nowUseringfo = UserInfoGet();
            IEnumerable<ChatUser> result = _connectlist.Where(x => x.UserId != nowUseringfo.UserId);
            return result;
        }

        public override async Task OnConnectedAsync()
        {
            string signalRconnectid = Context.ConnectionId;
            ChatUser nowUseringfo = UserInfoGet();

            if (nowUseringfo!=null && !(_connectlist.Any(x => x.UserId == nowUseringfo.UserId)))
            {
                _connectlist.Add(nowUseringfo);
                await Clients.All.SendAsync("UserConnected", nowUseringfo);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            string nowUserid = Context.ConnectionId;
            ChatUser nowUseringfo = UserInfoGet();
            ChatUser? user = _connectlist.SingleOrDefault(x => x.UserId == nowUseringfo.UserId);

            if (user != null)
            {
                _connectlist.Remove(user);
                await Clients.All.SendAsync("UserDisconnected", nowUseringfo.UserId);
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
            ChatUser nowUseringfo = UserInfoGet();
            ChatUser? user = _connectlist.SingleOrDefault(x=>x.UserId == nowUseringfo.UserId);
            await Clients.Client(sendid).SendAsync("PrivateMessage", user, message);
        }

        public IEnumerable<ChatUser> OnlineUser_Get()
        {
            return _connectlist;
        }

        private ChatUser UserInfoGet() 
        {
            ChatUser user = null;
            var httpContext = Context.GetHttpContext();
            var userIdClaim = httpContext.User.Claims;
            if (userIdClaim != null && userIdClaim.Count()>0)
            {
                user = new ChatUser
                {
                    UserName = userIdClaim!?.FirstOrDefault(c => c.Type == "UserName") != null ? userIdClaim.FirstOrDefault(c => c.Type == "UserName").Value.ToString() : "",
                    UserId = userIdClaim.FirstOrDefault(c => c.Type == "UserID") != null ? userIdClaim.FirstOrDefault(c => c.Type == "UserID").Value.ToString() : "",
                    imgPath = userIdClaim.FirstOrDefault(c => c.Type == "PicturesPath") != null ? userIdClaim.FirstOrDefault(c => c.Type == "PicturesPath").Value.ToString() : "https://fakeimg.pl/80/",
                };
            }

            return user;
        }
    }
}
