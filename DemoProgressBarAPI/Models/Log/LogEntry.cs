using DemoProgressBarAPI.Models.Enums;

namespace DemoProgressBarAPI.Models.Log
{
    /// <summary>
    /// 表示一個記錄訊息的請求。
    /// </summary>
    public class LogRequest
    {
        /// <summary>
        /// 記錄訊息。
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 記錄等級（例如：Info、Warning、Error、Debug）。
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// 呼叫記錄請求的端點。
        /// </summary>
        public CallEndEnum CallEnd { get; set; }
    }

    /// <summary>
    /// 表示具有附加資訊的記錄條目。
    /// </summary>
    public class LogEntry : LogRequest
    {
        /// <summary>
        /// 產生記錄的使用者ID。
        /// </summary>
        public string UserID { get; set; }
        public DateTime Timestamp { get; set; }
        public string LogLevelString => base.LogLevel.ToString();
        public string CallEndString => base.CallEnd.ToString();
    }
}
