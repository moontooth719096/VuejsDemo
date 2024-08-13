using DemoProgressBarAPI.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using static DemoProgressBarAPI.Controllers.GoogleAuthController;

namespace DemoProgressBarAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpGet("LoginCkeck")]
        public IActionResult LoginCkeck()
        {
            return Ok();
        }

        [HttpGet("v2/LoginCkeck")]
        public IActionResult LoginCkeckV2()
        {
            var userInfo = new UserInfo
            {
                UserID = User?.FindFirst("UserID")?.Value,
                UserName = User.FindFirst("UserName")?.Value,
                PicturesPath = User.FindFirst("PicturesPath")?.Value,
                UserLevel = int.TryParse(User.FindFirst("UserLevel")?.Value, out int level) ? level : 0
            };

            return Ok(userInfo);
        }

    }
}
