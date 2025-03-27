using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace DemoProgressBarAPI.Services
{
    public class LoggingService
    {
        private readonly string _logFilePath;
        private readonly string _logfilename;
        private string _folderPath;
        private string _filePath;

        public LoggingService(string logFilePath,string logfilename)
        {
            _logFilePath = logFilePath;
            _logfilename = logfilename;

            _folderPath = Path.Combine(Directory.GetCurrentDirectory(), _logFilePath);
            _filePath = Path.Combine(_folderPath, _logfilename);
        }

        public void Log(string message)
        {
            try
            {
                // 檢查文件夾是否存在
                FileCheck(_folderPath);
                var logMessage = $"{DateTime.Now}: {message}";
                File.AppendAllText(_filePath, logMessage + Environment.NewLine);

            }
            catch (Exception ex)
            { 
            
            }
        }

        public void FileCheck(string folderPath)
        {
            // 检查文件夹是否存在
            if (!Directory.Exists(folderPath))
                // 如果文件夹不存在，则创建新文件夹
                Directory.CreateDirectory(folderPath);
        }

        public async Task<List<LogEntry>> ReadLogAsync()
        {
            var logEntries = new List<LogEntry>();
            try
            {
                if (File.Exists(_filePath))
                {
                    var logContents = await File.ReadAllLinesAsync(_filePath);
                    var logEntryPattern = new Regex(@"^(?<timestamp>.+?): (?<message>.+)$");

                    foreach (var logLine in logContents)
                    {
                        var match = logEntryPattern.Match(logLine);
                        if (match.Success)
                        {
                            logEntries.Add(new LogEntry
                            {
                                Timestamp = DateTime.Parse(match.Groups["timestamp"].Value),
                                Message = match.Groups["message"].Value
                            });
                        }
                    }
                    // Sort log entries from newest to oldest
                    logEntries = logEntries.OrderByDescending(entry => entry.Timestamp).ToList();
                }
            }
            catch (Exception ex)
            {
                logEntries.Add(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    Message = $"Error reading log file: {ex.Message}"
                });
            }
            return logEntries;
        }
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
    }
}
