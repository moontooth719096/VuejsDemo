namespace DemoProgressBarAPI.Models.Log
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string UserID { get; set; }
        public string LogLevel { get; set; }
        public string Message { get; set; }
    }
}
