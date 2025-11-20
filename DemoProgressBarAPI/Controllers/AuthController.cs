using DemoProgressBarAPI.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using static DemoProgressBarAPI.Controllers.GoogleAuthController;
using DemoProgressBarAPI.Services;

namespace DemoProgressBarAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly LoggingService _loggingService;

        public AuthController(LoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        [HttpGet("LoginCkeck")]
        public IActionResult LoginCkeck()
        {
            return Ok();
        }

        //[HttpGet("v2/LoginCkeck")]
        //public IActionResult LoginCkeckV2()
        //{
        //    var userId = User?.FindFirst("UserID")?.Value;
        //    var userLevel = userId == "101349011586052745096" ? 99 : 0;

        //    var userInfo = new UserInfo
        //    {
        //        UserID = userId,
        //        UserName = User.FindFirst("UserName")?.Value,
        //        PicturesPath = User.FindFirst("PicturesPath")?.Value,
        //        UserLevel = userLevel
        //    };

        //    return Ok(userInfo);
        //}

        [HttpGet("v2/LoginCkeck")]
        public IActionResult LoginCkeckV2()
        {
            var userInfo = new UserInfo
            {
                UserID = User?.FindFirst("UserID")?.Value,
                UserName = User.FindFirst("UserName")?.Value,
                PicturesPath = User.FindFirst("PicturesPath")?.Value,
                UserLevel = int.TryParse(User.FindFirst("UserLevel")?.Value, out int level) ? level : 0,
                ThirdPlatform = User.FindFirst("ThirdPlatform")?.Value,
                ThirdToken = User.FindFirst("ThirdToken")?.Value
            };

            return Ok(userInfo);
        }

    }
}
