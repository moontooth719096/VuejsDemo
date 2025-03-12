using Microsoft.AspNetCore.SignalR;

namespace DemoProgressBarAPI.Utils
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            var userIdClaim = connection.User?.FindFirst(c => c.Type == "UserID");
            return userIdClaim?.Value;
        }
    }
}
