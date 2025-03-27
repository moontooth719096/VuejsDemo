using DemoProgressBarAPI.Interfaces;
using DemoProgressBarAPI.Services;
using Google.Apis.Auth;
using Google.Apis.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace DemoProgressBarAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GoogleAuthController : ControllerBase
    {
        private readonly IGoogleOAuthService _googleOAuthService;
        private readonly LoggingService _loggingService;

        public GoogleAuthController(IGoogleOAuthService googleOAuthService, LoggingService loggingService)
        {
            _googleOAuthService = googleOAuthService;
            _loggingService = loggingService;
        }
        public class GoogleLoginRequest
        {
            public string? Code { get; set; }
            public string Credential { get; set; }
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login(GoogleLoginRequest request)
        {
            //string? formCredential = Request.Form["credential"]; //回傳憑證
            //string? formToken = Request.Form["g_csrf_token"]; //回傳令牌
            //string? cookiesToken = Request.Cookies["g_csrf_token"]; //Cookie 令牌
            try
            {
                return Ok(await _googleOAuthService.Verify(request.Credential));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
