using DemoProgressBarAPI.Interfaces;
using DemoProgressBarAPI.Models;
using DemoProgressBarAPI.Models.User;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace DemoProgressBarAPI.Services
{
    public class GoogleOAuthService : IGoogleOAuthService
    {
        private IConfiguration _configuration;
        private JWTSettings _jwt;
        public GoogleOAuthService(IConfiguration configuration, IOptions<JWTSettings> jwt)
        {
            _configuration = configuration;
            _jwt = jwt.Value;
    }
        /// <summary>
        /// 驗證 Google Token
        /// </summary>
        /// <param name="formCredential"></param>
        /// <param name="formToken"></param>
        /// <param name="cookiesToken"></param>
        /// <returns></returns>
        public async Task<string?> Verify(string? formCredential)
        {
            string JWT = string.Empty;
            try
            {
                // 檢查空值
                if (formCredential == null)
                {
                    return null;
                }

                GoogleJsonWebSignature.Payload? payload = await GoogleVerify(formCredential);
                //整理需要的資料
                UserInfo userInfo = new UserInfo
                {
                    UserName = payload.Name,
                    UserID = payload.Subject,
                    PicturesPath = payload.Picture,
                };
                //產生自己的JWT
                JWT = CreateJwtToken(userInfo);
            }
            catch (Exception ex)
            { 
            
            }

            return JWT;
        }
        private async Task<Payload> GoogleVerify(string? formCredential)
        {
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
                return payload;
            }
            catch
            {
                return null;
            }
        }

        private string CreateJwtToken(UserInfo user)
        {

            var key = Encoding.ASCII.GetBytes(_jwt.Secret);

            var userClaims = BuildUserClaims(user);

            var signKey = new SymmetricSecurityKey(key);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.ValidIssuer,
                notBefore: DateTime.UtcNow,
                audience: _jwt.ValidAudience,
                expires: DateTime.UtcNow.AddDays(Convert.ToInt32(_jwt.DurationInDay)),
                claims: userClaims,
                signingCredentials: new SigningCredentials(signKey, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }

        private List<Claim> BuildUserClaims(UserInfo user)
        {
            List<Claim> userClaims = user.GetType().GetProperties().Select(x=>new Claim(x.Name,x.GetValue(user)?.ToString()??string.Empty)).ToList();
            userClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

            return userClaims;
        }
    }
}
