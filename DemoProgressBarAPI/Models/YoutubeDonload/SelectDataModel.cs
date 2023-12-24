using YoutubeExplode.Videos;

namespace DemoProgressBarAPI.Models.YoutubeDonload
{
    public class SelectDataModel
    {
        public string id { get; set; }
        public VideoId VideoId => VideoId.Parse(id);
        public string Title { get; set; }
    }
}
