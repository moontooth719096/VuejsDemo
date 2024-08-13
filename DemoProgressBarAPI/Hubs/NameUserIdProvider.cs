using Microsoft.AspNetCore.SignalR;

namespace DemoProgressBarAPI.Hubs
{
    public class NameUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            // 認證設定時設置好 NameClaimType，這裡直接回傳 User.Identity.Name 即可
            return connection.User?.Identity?.Name;
        }
    }
}
