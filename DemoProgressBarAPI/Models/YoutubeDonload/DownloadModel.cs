namespace DemoProgressBarAPI.Models.YoutubeDonload
{
    public class DownloadModel
    {
        public string ConnectionId {  get; set; }
        public IEnumerable<SelectDataModel> SelectData { get; set; }
        public int Mode { get; set; }
        public  string GAuthToken { get; set; }
       public string SelectFileID { get; set; }
    }
}
