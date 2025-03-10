using DemoProgressBarAPI.Models.ChatRoom;
using DemoProgressBarAPI.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace DemoProgressBarAPI.Hubs
{
    public class YoutubeDownloadProgressHub : HubBase
    {
        //private static ConcurrentDictionary<string, UserInfo> _connentUser = new ConcurrentDictionary<string, UserInfo>();
        public override async Task OnConnectedAsync()
        {
            //UserInfo nowUseringfo = UserInfoGet();
            //if (nowUseringfo != null)
            //    _connentUser.TryAdd(nowUseringfo.UserID, nowUseringfo);
            
            await base.OnConnectedAsync();
        }

        //public override async Task OnDisconnectedAsync(Exception? ex)
        //{
        //    UserInfo nowUseringfo = UserInfoGet();
        //    if (nowUseringfo != null)
        //    {
        //        _connentUser.TryRemove(nowUseringfo.UserID, out _);
        //    }

        //    await base.OnDisconnectedAsync(ex);
        //}

        //public async Task SendProgressUpdate(string userid, string message, int percentage)
        //{
        //    if (_connentUser.TryGetValue(userid, out UserInfo valueForExistingKey))
        //    {
        //        await Clients.Client(valueForExistingKey.ConnectionID).SendAsync("YoutubeDownloadProgress", message, percentage);
        //    }
        //}

        //public async Task NotifyDownloadCompleted(string userid, string downloadUrl)
        //{
        //    if (_connentUser.TryGetValue(userid, out UserInfo valueForExistingKey))
        //    {
        //        await Clients.Client(valueForExistingKey.ConnectionID).SendAsync("YoutubeDownloadCompleted", userid, downloadUrl);
        //    }
        //}
    }
}
