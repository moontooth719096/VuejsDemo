namespace DemoProgressBarAPI.Models.YoutubeDonload
{
    public class DownloadModel
    {
        public string ConnectionId {  get; set; }
        public IEnumerable<SelectDataModel> SelectData { get; set; }
    }
}
