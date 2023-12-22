using DemoProgressBarAPI.Interfaces;
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

        public GoogleAuthController(IGoogleOAuthService googleOAuthService)
        {
            _googleOAuthService = googleOAuthService;
        }
        public class GoogleLoginRequest
        {
            public string? Code { get; set; }
            public string Credential { get; set; }
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
                JWTToken = _googleOAuthService.Verify(request.Credential).Result;
          
                return Content(JWTToken);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
    }
}
