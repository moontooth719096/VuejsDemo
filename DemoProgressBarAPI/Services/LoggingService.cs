using Newtonsoft.Json.Linq;
using System;
using System.IO;

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
    }
}
