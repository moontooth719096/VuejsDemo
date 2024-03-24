using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DemoProgressBarAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
  
    public class ChatRoomController : ControllerBase
    {
        [HttpGet("OnlineUser_Get")]
        public IActionResult OnlineUser_Get()
        {
            return Ok();
        }
    }
}
