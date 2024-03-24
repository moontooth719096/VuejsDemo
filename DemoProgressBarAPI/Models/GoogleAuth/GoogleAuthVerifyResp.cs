using DemoProgressBarAPI.Models.User;

namespace DemoProgressBarAPI.Models.GoogleAuth
{
    public class GoogleAuthVerifyResp
    {
        public string JWT { get; set; }
        public UserInfo userInfo { get; set; }
    }
}
