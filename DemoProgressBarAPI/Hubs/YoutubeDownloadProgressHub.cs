﻿using Microsoft.AspNetCore.SignalR;

namespace DemoProgressBarAPI.Hubs
{
    public class YoutubeDownloadProgressHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine(Context.ConnectionId);
            await base.OnConnectedAsync();
        }
        public async Task SendProgressUpdate(string message, int percentage)
        {
            await Clients.All.SendAsync("YoutubeDownloadProgress", message, percentage);
        }
    }
}
