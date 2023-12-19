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
        private IConfiguration _configuration;
        public GoogleAuthController(IConfiguration configuration)
        {
            _configuration = configuration;
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
                GoogleJsonWebSignature.Payload? payload = VerifyGoogleToken(request.credential).Result;
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
        /// <summary>
        /// 驗證 Google Token
        /// </summary>
        /// <param name="formCredential"></param>
        /// <param name="formToken"></param>
        /// <param name="cookiesToken"></param>
        /// <returns></returns>
        private async Task<GoogleJsonWebSignature.Payload?> VerifyGoogleToken(string? formCredential)
        {
            // 檢查空值
            if (formCredential == null)
            {
                return null;
            }

            GoogleJsonWebSignature.Payload? payload;
            try
            {
                // 驗證憑證
                string GoogleApiClientId = _configuration["GoogleAuth:ClientID"];
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>() { GoogleApiClientId }
                };
                payload = await GoogleJsonWebSignature.ValidateAsync(formCredential, settings);
                if (!payload.Issuer.Equals("accounts.google.com") && !payload.Issuer.Equals("https://accounts.google.com"))
                {
                    return null;
                }
                if (payload.ExpirationTimeSeconds == null)
                {
                    return null;
                }
                else
                {
                    DateTime now = DateTime.Now.ToUniversalTime();
                    DateTime expiration = DateTimeOffset.FromUnixTimeSeconds((long)payload.ExpirationTimeSeconds).DateTime;
                    if (now > expiration)
                    {
                        return null;
                    }
                }
            }
            catch
            {
                return null;
            }
            return payload;
        }
    }
}
