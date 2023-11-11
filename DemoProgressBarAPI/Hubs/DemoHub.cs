using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;

namespace DemoProgressBarAPI.Hubs
{
    public class DemoHub : Hub
    {
        public async Task ReceiveMessage(int percentage)
        {
            await Clients.All.SendAsync("ReceiveMessage", percentage);
        }
    }
}
