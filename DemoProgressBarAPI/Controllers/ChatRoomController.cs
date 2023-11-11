using Microsoft.AspNetCore.Mvc;

namespace DemoProgressBarAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatRoomController : ControllerBase
    {
        [HttpGet]
        public IActionResult OnlineUser_Get()
        {

            return Ok();
        }
    }
}
