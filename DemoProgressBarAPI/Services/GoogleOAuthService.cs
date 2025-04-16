using DemoProgressBarAPI.Interfaces;
using DemoProgressBarAPI.Models;
using DemoProgressBarAPI.Models.GoogleAuth;
using DemoProgressBarAPI.Models.User;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using YoutubeExplode.Channels;
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
        public async Task<GoogleAuthVerifyResp> Verify(string? formCredential)
        {
            GoogleAuthVerifyResp result = null;
            try
            {
                // 檢查空值
                if (formCredential == null)
                {
                    return null;
                }

                GoogleJsonWebSignature.Payload? payload = await GoogleVerify(formCredential);
                if (payload == null)
                    return result;
                //整理需要的資料
                UserInfo userInfo = new UserInfo
                {
                    UserName = payload.Name,
                    UserID = payload.Subject,
                    PicturesPath = payload.Picture,
                    ThirdPlatform = "Google",
                    ThirdToken = formCredential
                };

                if (userInfo != null)
                {
                    result = new GoogleAuthVerifyResp
                    {
                        userInfo = userInfo,
                        JWT = CreateJwtToken(userInfo)
                    };
                }
            }
            catch (Exception ex)
            {

            }

            return result;
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

            user.UserLevel = (user.UserID == "101349011586052745096" ? 99 : 0);
            List<Claim> userClaims = user.GetType().GetProperties().Select(x => new Claim(x.Name, x.GetValue(user)?.ToString() ?? string.Empty)).ToList();
            //List < Claim > userClaims = new List < Claim >();
            //userClaims.Add(new Claim(JwtRegisteredClaimNames.Name, user.UserName));
            //userClaims.Add(new Claim(JwtRegisteredClaimNames.NameId, user.UserID));
            //userClaims.Add(new Claim("PicturesPath", user.PicturesPath));
            //userClaims.Add(new Claim("UserLevel", user.UserLevel.ToString()));
            userClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
             
            return userClaims;
        }

        public ClaimsPrincipal GetClaimsPrincipalFromToken(string token, string signingKey)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(signingKey)),
                ValidateIssuer = false,
                ValidateAudience = false
            };

            try
            {
                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch (Exception ex)
            {
                // 处理异常
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
