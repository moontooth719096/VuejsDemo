using DemoProgressBarAPI.Interfaces;
using DemoProgressBarAPI.Models.User;
using Google.Apis.Auth;
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
        public GoogleOAuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        /// <summary>
        /// 驗證 Google Token
        /// </summary>
        /// <param name="formCredential"></param>
        /// <param name="formToken"></param>
        /// <param name="cookiesToken"></param>
        /// <returns></returns>
        public async Task<Payload?> Verify(string? formCredential)
        {
            // 檢查空值
            if (formCredential == null)
            {
                return null;
            }

            GoogleJsonWebSignature.Payload? payload = await GoogleVerify(formCredential);
            //整理需要的資料
            UserInfo userInfo = new UserInfo { 
                UserName = payload.Name,
                UserID = payload.Subject,
                PicturesPath = payload.Picture,
            };
            //產生自己的JWT
            string JWT = CreateJwtToken(userInfo);

            return payload;
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
                expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(_jwt.DurationInMinutes)),
                claims: userClaims,
                signingCredentials: new SigningCredentials(signKey, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }

        private List<Claim> BuildUserClaims(UserInfo user)
        {
            List<Claim> userClaims = user.GetType().GetProperties().Select(x=>new Claim(x.Name,x.GetValue(user)?.ToString()??string.Empty)).ToList();
            userClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));


        //     var userClaims = new List<Claim>()
        //{ClaimTypes
        //    new Claim(ClaimTypes.Name, user.Id.ToString()),
        //    new Claim(JwtClaimTypes.Email, user.Email),
        //    new Claim(JwtClaimTypes.GivenName, user.FirstName),
        //    new Claim(JwtClaimTypes.FamilyName, user.LastName),
        //    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        //};

            return userClaims;
        }
    }
}
