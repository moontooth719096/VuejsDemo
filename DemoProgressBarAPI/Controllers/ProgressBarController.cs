using DemoProgressBarAPI.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace DemoProgressBarAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProgressBarController : ControllerBase
    {
        private IHubContext<DemoHub> _demoHub;
        public ProgressBarController(IHubContext<DemoHub> demoHub)
        {
            _demoHub = demoHub;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int percentage = 0;

            while (percentage<100)
            {
                //CALCULATING PERCENTAGE BASED ON THE PARAMETERS SENT
                percentage = percentage+1;
                //PUSHING DATA TO ALL CLIENTS
                await _demoHub.Clients.All.SendAsync("ReceiveMessage", percentage);

                await Task.Delay(100);
            }

            return Ok();
        }
    }
}
