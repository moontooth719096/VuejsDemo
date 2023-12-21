namespace DemoProgressBarAPI.Models
{
    public class JWTSettings
    {
        public string ValidAudience { get; set; }
        public string ValidIssuer { get; set; }
        public int DurationInDay { get; set; }
        public string Secret { get; set; }
    }

}
