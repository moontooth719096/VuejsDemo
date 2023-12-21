using Google.Apis.Auth;

namespace DemoProgressBarAPI.Interfaces
{
    public interface IGoogleOAuthService
    {
        Task<string> Verify(string? formCredential);
    }
}