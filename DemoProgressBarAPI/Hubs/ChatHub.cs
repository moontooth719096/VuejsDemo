using DemoProgressBarAPI.Models.ChatRoom;
using DemoProgressBarAPI.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using DemoProgressBarAPI.Services;
using YoutubeExplode.Channels;

namespace DemoProgressBarAPI.Hubs
{
    [Authorize]
    public class ChatHub : HubBase
    {
        private static List<ChatUser> _connectlist = new List<ChatUser>();
        private readonly LoggingService _loggingService;

        public ChatHub(LoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        public override async Task OnConnectedAsync()
        {
            UserInfo nowUseringfo = UserInfoGet();

            if (nowUseringfo != null)
            {
                if (_connectlist.Any(x => x.UserID == nowUseringfo.UserID))
                {
                    _connectlist.Remove(_connectlist.Single(x => x.UserID == nowUseringfo.UserID));
                }
                _connectlist.Add(new ChatUser(nowUseringfo));
                    
                _loggingService.Log($"Client connected，{nowUseringfo.UserName}");
                await Clients.All.SendAsync("RefreshConnectList", _connectlist);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? ex)
        {
            UserInfo nowUseringfo = UserInfoGet();

            ChatUser? user = _connectlist.SingleOrDefault(x => x.UserID == nowUseringfo.UserID);
            if (user!=null)
            {
                _connectlist.Remove(user);
                await Clients.All.SendAsync("UserDisconnected", nowUseringfo.UserID);
            }
            _loggingService.Log($"Client Disconnected，{nowUseringfo.UserName}");
            await base.OnDisconnectedAsync(ex);
        }

        public async Task WorldMessage(string connectid, string message)
        {
            await Clients.All.SendAsync("WorldMessage", connectid, message);
        }

        public async Task PrivateMessage(string sendid, string message)
        {
            ChatUser? senduser = _connectlist.SingleOrDefault(x => x.UserID == sendid);
            UserInfo nowUseringfo = UserInfoGet();
            ChatUser? nowuser = _connectlist.SingleOrDefault(x => x.UserID == nowUseringfo.UserID);
            if (senduser != null)
                await Clients.Users(senduser.UserID).SendAsync("PrivateMessage", nowuser, message);
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

        // 供前端主動呼叫的方法，取得目前所有連線的使用者
        public async Task<List<ChatUser>> GetConnectedUsers()
        {
            return await Task.FromResult(_connectlist);
        }
    }
}
