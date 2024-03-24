using DemoProgressBarAPI.Models.GoogleAuth;
using Google.Apis.Auth;

namespace DemoProgressBarAPI.Interfaces
{
    public interface IGoogleOAuthService
    {
        Task<GoogleAuthVerifyResp> Verify(string? formCredential);
    }
}