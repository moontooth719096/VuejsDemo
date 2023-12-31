﻿using DemoProgressBarAPI.Models.ChatRoom;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DemoProgressBarAPI.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        //private static List<string> _connectlist;
        private static List<ChatUser> _connectlist = new List<ChatUser>();

        public override async Task OnConnectedAsync()
        {
            string nowUserid = Context.ConnectionId;

            List<ChatUser> nowonlineusers = new List<ChatUser>();
            nowonlineusers.AddRange(_connectlist);

            if (!_connectlist.Any(x => x.ConnectionID == nowUserid))
            {
                ChatUser user = new ChatUser
                {
                    ConnectionID = nowUserid
                };

                _connectlist.Add(user);
                await Clients.All.SendAsync("UserConnected", user);
            }

            if (nowonlineusers != null && nowonlineusers.Count > 0)
                await Clients.Client(nowUserid).SendAsync("OnlineList", nowonlineusers);


            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            string nowUserid = Context.ConnectionId;
            ChatUser? user = _connectlist.SingleOrDefault(x => x.ConnectionID == nowUserid);

            if(user!=null)
                _connectlist.Remove(user);

            await Clients.All.SendAsync("UserDisconnected", nowUserid);

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
            await Clients.Client(sendid).SendAsync("PrivateMessage", user, message);
        }

        public IEnumerable<ChatUser> OnlineUser_Get()
        {
            return _connectlist;
        }
    }
}
