using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.Extensions.Hosting;
using DemoProgressBarAPI.Models.Log;
using Microsoft.AspNetCore.Http;
using DemoProgressBarAPI.Models.Enums;

namespace DemoProgressBarAPI.Services
{
    public class LoggingService
    {
        private readonly string _logFilePath = "logs";
        private readonly string _logfilename = "log.txt";
        private string _folderPath;
        private string _filePath;
        private readonly IHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoggingService(IHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
            _folderPath = Path.Combine(Directory.GetCurrentDirectory(), _logFilePath);
            _filePath = Path.Combine(_folderPath, _logfilename);
        }

        public void ApiLog(string message, LogLevel loglevel = LogLevel.Information)
        {
            Log(message, CallEndEnum.Demoapp2Backend, loglevel);
        }
        public void Log(string message, CallEndEnum callend, LogLevel loglevel = LogLevel.Information)
        {
            try
            {
                // 如果不是開發環境且log等級是Debug，則不紀錄
                if (!_env.IsDevelopment() && loglevel == LogLevel.Debug)
                {
                    return;
                }

                // 檢查文件夾是否存在
                FileCheck(_folderPath);

                var userId = _httpContextAccessor.HttpContext?.User?.FindFirst("UserID")?.Value ?? "";
                var logMessage = $"{DateTime.Now}|{userId}|{loglevel.ToString()}|{message}|{callend.ToString()}";
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
                    string[] logContents = await File.ReadAllLinesAsync(_filePath);

                    foreach (var logLine in logContents)
                    {
                        if (!string.IsNullOrEmpty(logLine))
                        {
                            string[] strings = logLine.Split('|');
                            logEntries.Add(new LogEntry
                            {
                                Timestamp = DateTime.Parse(strings[0]),
                                UserID = strings[1],
                                LogLevel = Enum.TryParse<LogLevel>(strings[2], out LogLevel logLevel) ? logLevel : LogLevel.None,
                                Message = strings[3],
                                CallEnd = Enum.TryParse<CallEndEnum>(strings[4], out CallEndEnum callend) ? callend : CallEndEnum.None,
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
}
