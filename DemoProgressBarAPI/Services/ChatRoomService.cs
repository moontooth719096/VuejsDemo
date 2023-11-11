using DemoProgressBarAPI.Hubs;
using Microsoft.AspNet.SignalR.Messaging;
using Microsoft.AspNetCore.SignalR;

namespace DemoProgressBarAPI.Services
{
    public class ChatRoomService
    {
        private IHubContext<ChatHub> _chatroomservice;
        public ChatRoomService(IHubContext<ChatHub> chatroomservice)
        {
            _chatroomservice = chatroomservice;
        }

        //public IEnumerable<> GetOnlineusers()
        //{
        //    _chatroomservice.Clients.;
        //}
    }
}
