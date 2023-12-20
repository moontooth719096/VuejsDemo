using DemoProgressBarAPI.Interfaces;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace DemoProgressBarAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GoogleAuthController : ControllerBase
    {
        private readonly IGoogleOAuthService _googleOAuthService;

        public GoogleAuthController(IGoogleOAuthService googleOAuthService)
        {
            _googleOAuthService = googleOAuthService;
        }
        public class GoogleLoginRequest
        {
            public string? Code { get; set; }
            public string credential { get; set; }
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(GoogleLoginRequest request)
        {
            //string? formCredential = Request.Form["credential"]; //回傳憑證
            //string? formToken = Request.Form["g_csrf_token"]; //回傳令牌
            //string? cookiesToken = Request.Cookies["g_csrf_token"]; //Cookie 令牌
            try
            {
                string JWTToken = string.Empty;
                // 驗證 Google Token
                GoogleJsonWebSignature.Payload? payload = _googleOAuthService.Verify(request.credential).Result;
                if (payload == null)
                {
                    // 驗證失敗
                }
                return Content(JWTToken);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
    }
}
