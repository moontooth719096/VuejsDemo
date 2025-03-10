using DemoProgressBarAPI.Models.ChatRoom;
using DemoProgressBarAPI.Models.User;
using Microsoft.AspNetCore.SignalR;

namespace DemoProgressBarAPI.Hubs
{
    public class HubBase:Hub
    {

        protected UserInfo UserInfoGet()
        {
            UserInfo user = null;
            //取得目前連線id
            string signalRconnectid = Context.ConnectionId;
            var httpContext = Context.GetHttpContext();
            //取得JWT內使用者資訊
            var userIdClaim = httpContext?.User?.Claims;
            //var claims = ((ClaimsIdentity)Context.User.Identity).Claims;
            if (userIdClaim != null && userIdClaim.Count() > 0)
            {
                user = new UserInfo
                {
                    UserName = userIdClaim!?.FirstOrDefault(c => c.Type == "UserName") != null ? userIdClaim.FirstOrDefault(c => c.Type == "UserName").Value.ToString() : "",
                    UserID = userIdClaim.FirstOrDefault(c => c.Type == "UserID") != null ? userIdClaim.FirstOrDefault(c => c.Type == "UserID").Value.ToString() : "",
                    PicturesPath = userIdClaim.FirstOrDefault(c => c.Type == "PicturesPath") != null ? userIdClaim.FirstOrDefault(c => c.Type == "PicturesPath").Value.ToString() : "https://fakeimg.pl/80/",
                    ConnectionID = signalRconnectid
                };
            }

            return user;
        }
    }
}
