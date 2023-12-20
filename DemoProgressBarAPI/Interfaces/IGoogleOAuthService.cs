using Google.Apis.Auth;

namespace DemoProgressBarAPI.Interfaces
{
    public interface IGoogleOAuthService
    {
        Task<GoogleJsonWebSignature.Payload?> Verify(string? formCredential);
    }
}