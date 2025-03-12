using DemoProgressBarAPI.Models.ChatRoom;
using DemoProgressBarAPI.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace DemoProgressBarAPI.Hubs
{
    [Authorize]
    public class YoutubeDownloadProgressHub : HubBase
    {
        //private static ConcurrentDictionary<string, UserInfo> _connentUser = new ConcurrentDictionary<string, UserInfo>();
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
    }
}
