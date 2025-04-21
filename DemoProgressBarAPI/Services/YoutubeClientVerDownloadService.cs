using DemoProgressBarAPI.Hubs;
using DemoProgressBarAPI.Interfaces;
using DemoProgressBarAPI.Models.YoutubeDonload;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NAudio.Wave;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace DemoProgressBarAPI.Services
{
    public class YoutubeClientVerDownloadService : YoutubeDonloadBaseService, IYoutubeListDownloadService
    {
        private readonly YoutubeClient _youtubeclient;
        private readonly IHubContext<YoutubeDownloadProgressHub> _youtubeDownloadProgressHub;
        private readonly LoggingService _loggingService;

        public YoutubeClientVerDownloadService(
            IHubContext<YoutubeDownloadProgressHub> youtubeDownloadProgressHub
            , LoggingService loggingService)
        {
            _youtubeDownloadProgressHub = youtubeDownloadProgressHub;
            _youtubeclient = new YoutubeClient();
            _loggingService = loggingService;
        }

        public async IAsyncEnumerable<YotubeDownloadListViewModel> VideoGet(string VideoID)
        {
            //取得youtube清單
            Video Music = await SearchYoutubeClientVer_Get(VideoID);
           
            if (Music != null)
            {
                yield return new YotubeDownloadListViewModel
               {
                   IsCheck = true,
                   Title = Music.Title,
                   Id = Music.Id,
                   ThumbnailUrl = Music.Thumbnails.SingleOrDefault(Thumbnail => Thumbnail.Resolution.Area == Music.Thumbnails.Max(Thumbnail => Thumbnail.Resolution.Area)).Url,
                   PlayTime = Music.Duration.ToString()
               };
            }
        }

        public async IAsyncEnumerable<YotubeDownloadListViewModel> PlayListGet(string PlaylistId)
        {
            //取得youtube清單
            IEnumerable<PlaylistVideo> musicList = await SearchListYoutubeClientVer_Get(PlaylistId);
            if (musicList != null)
            {
                foreach (var value in musicList)
                {
                     yield return new YotubeDownloadListViewModel
                    {
                        IsCheck = true,
                        Title = value.Title,
                        Id = value.Id,
                        ThumbnailUrl = value?.Thumbnails?.SingleOrDefault(thumbnail => thumbnail.Resolution.Area == value.Thumbnails.Max(thumbnail => thumbnail.Resolution.Area))?.Url,
                        PlayTime = value.Duration.ToString()
                    };
                }
            }
        }

        public async Task<string> DownloadApp(DownloadModel downloadData)
        {
            if (string.IsNullOrEmpty(downloadData.ConnectionId))
                throw new Exception("connectionid is empty");

            string token = DateTime.Now.ToString("yyyyMMddHHmmssffff");
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "YoutubeDonload", token);

            // 檢查文件夾是否存在
            FileCheck(folderPath);
            bool isgoogledl = downloadData.Mode==1?false:true;
            // 將下載和壓縮的工作移到後台任務中
            _ = Task.Run(async () => await ProcessDownloadAndCompression(isgoogledl,downloadData.ConnectionId, downloadData.SelectData, folderPath, token, downloadData.GAuthToken, downloadData.SelectFileID));

            // 返回任務ID給前端
            return token;
        }

        private async Task ProcessDownloadAndCompression(bool isgoogleDL,  string connectionid,IEnumerable<SelectDataModel> SelectData, string folderPath, string token,string gauthtoken = "",string selectfileid="")
        {
            List<Task> downloadList = new List<Task>();
            List<MP4Streaminfo> videos = new List<MP4Streaminfo>();
            List<SelectDataModel> doList = SelectData.OrderBy(x => x.id).ToList();
            int takeCount = 5;
            double totalCount = doList.Count();
            string message = "音樂下載中";
            double percentage = 0;

            while (doList.Count > 0)
            {
                IEnumerable<SelectDataModel> nowList = doList.Take(takeCount);
                foreach (var searchResult in nowList)
                {
                    string filename = $"{searchResult.Title}.mp4";
                    filename = Regex.Replace(filename, "[\\/:*?\"<>|]", "").Replace(" ", "");

                    downloadList.Add(DownloadAndConvertVideo(searchResult, folderPath, filename, videos));
                }
                await Task.WhenAll(downloadList);
                doList.RemoveRange(0, nowList.Count());
                percentage = 100 - (Math.Round((doList.Count / totalCount) * 100));
                await UpdateProgress(connectionid, message, percentage);
            }

            await ConvertToMP3(connectionid, videos);
            await UpdateProgress(connectionid, "檔案壓縮中", 99);
            string zipFileName = $"compressed-files-{token}.zip";
           string zipfilepath =  await ZipDownloadFile(zipFileName, folderPath);

            if (isgoogleDL)
            {
                await UploadToGoogleDrive(zipfilepath, zipFileName, gauthtoken, selectfileid);
            }

            await _youtubeDownloadProgressHub.Clients.User(connectionid).SendAsync("YoutubeDownloadCompleted", zipFileName, $"/YoutubeDonloadZIP/{zipFileName}");
           
        }

        private async Task DownloadAndConvertVideo(SelectDataModel searchResult, string folderPath, string filename, List<MP4Streaminfo> videos)
        {
            try
            {
                MP4Streaminfo videoInfo = await VedioStream_Get(searchResult.VideoId, folderPath, filename);
                if (videoInfo != null)
                    videos.Add(videoInfo);
            }
            catch (Exception ex)
            {
                // 記錄異常信息
                _loggingService.ApiLog($"下載 {filename} 發生錯誤: {ex.ToString()}",LogLevel.Error);
            }
        }

        private async Task UpdateProgress(string connectionID, string message, double percentage)
        {
            await _youtubeDownloadProgressHub.Clients.User(connectionID).SendAsync("YoutubeDownloadProgress", message, percentage);
        }

        private async Task<string> ZipDownloadFile(string zipFileName, string tragePath)
        {
            string zipFilePath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "YoutubeDonloadZIP");
            string AllzipFilePath = Path.Combine(zipFilePath, zipFileName);

            // 检查文件夹是否存在
            FileCheck(zipFilePath);

            try
            {
                // 压缩临时文件夹中的所有文件
                ZipFile.CreateFromDirectory(tragePath, AllzipFilePath);
            }
            catch (Exception ex)
            {
                _loggingService.ApiLog($"壓縮 {zipFileName} 發生錯誤: {ex.ToString()}");
            }


            // 构建响应，将zip文件提供给客户端下载
            //var memoryStream = new MemoryStream();
            //using (var fs = new FileStream(AllzipFilePath, FileMode.Open))
            //{
            //    await fs.CopyToAsync(memoryStream);
            //}
            //memoryStream.Seek(0, SeekOrigin.Begin);

            //return new FileStreamResult(memoryStream, System.Net.Mime.MediaTypeNames.Application.Zip);
            return AllzipFilePath;

        }

        private async Task<Video> SearchYoutubeClientVer_Get(string VedioID)
        {
            //var playlistUrl = "https://youtube.com/playlist?list=" + PlaylistId;
            Video Result = null;
            try
            {
                Result = await _youtubeclient.Videos.GetAsync(VedioID);

            }
            catch (Exception ex)
            {
                _loggingService.ApiLog($"查詢VedioID：{VedioID} 發生錯誤: {ex.ToString()}", LogLevel.Error);
            }
            return Result;

        }

        private async Task<IEnumerable<PlaylistVideo>> SearchListYoutubeClientVer_Get(string PlaylistId)
        {
            var playlistUrl = "https://youtube.com/playlist?list=" + PlaylistId;
            IEnumerable<PlaylistVideo> Result = Enumerable.Empty<PlaylistVideo>();
            try 
            {
                Result = await _youtubeclient.Playlists.GetVideosAsync(playlistUrl);
            }
            catch (Exception ex)
            {
                _loggingService.ApiLog($"查詢PlaylistId：{PlaylistId} 發生錯誤: {ex.ToString()}",LogLevel.Error);
            }
            return Result;

        }
        private async Task<MP4Streaminfo> VedioStream_Get(VideoId item, string folderPath, string filename)
        {
            try
            {
                // 解析影片信息
                var video = await _youtubeclient.Videos.Streams.GetManifestAsync(item);

                // 擷取聲音(取最高音質
                var audioStreamInfo = video.GetAudioOnlyStreams().GetWithHighestBitrate();

                //設定要轉換的kbps
                int nowkbps = kbpsSet(audioStreamInfo.Bitrate.KiloBitsPerSecond);


                //取得stream
                var audioStream = await _youtubeclient.Videos.Streams.GetAsync(audioStreamInfo);
                
                
                String SaveMP3File = filename.Replace(".mp4", ".mp3");
                string MP3outPath = Path.Combine(folderPath, SaveMP3File);

                return new MP4Streaminfo { 
                    OutputPath = MP3outPath,
                    MP4Stream = audioStream,
                    Kbps = kbpsSet(audioStreamInfo.Bitrate.KiloBitsPerSecond)
                 };

            }
            catch (Exception ex)
            {
                _loggingService.ApiLog($"下載 {filename} 發生錯誤：{ex}", LogLevel.Error);
            }
            return null;
        }

        private int kbpsSet(double orgkbps)
        {
            int nowkbps = 0;
            //設定要轉換的kbps
            switch (orgkbps)
            {
                case <= 96://64kbps
                    nowkbps = 64;
                    break;
                case > 96 and <= 145://128kbps
                    nowkbps = 128;
                    break;
                case > 145 and <= 224://192kbps
                    nowkbps = 192;
                    break;
                case > 224 and <= 278://256kbps
                    nowkbps = 256;
                    break;
                case > 278://320kbps
                    nowkbps = 320;
                    break;
            }
            return nowkbps;
        }

        private async Task ConvertToMP3(string connectionid, List<MP4Streaminfo> videos)
        {
            string message = "格式轉換中";
            int count = 0;
            double TotalCount = videos.Count();
            double percentage = 0;
            await UpdateProgress(connectionid, message, 0);
            List<Task> tasks = new List<Task>();
            foreach (MP4Streaminfo video in videos)
            {
                tasks.Add(Task.Run(async () => {
                    try
                    {
                        string arguments = $"-i pipe:0 -b:a {video.Kbps}k \"{video.OutputPath}\"";
                        var processStartInfo = new ProcessStartInfo
                        {
                            FileName = "ffmpeg",
                            Arguments = arguments,
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        using (var process = new Process { StartInfo = processStartInfo })
                        {
                            process.Start();
                            await video.MP4Stream.CopyToAsync(process.StandardInput.BaseStream);
                            process.StandardInput.Close();
                            await process.WaitForExitAsync();
                        }

                        count++;
                        percentage = Math.Round((count / TotalCount) * 100);
                        await UpdateProgress(connectionid, message, percentage);
                        await Console.Out.WriteLineAsync($"已保存 MP3 文件至 {video.OutputPath}。").ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _loggingService.ApiLog($"轉檔發生異常：{video.OutputPath} 發生錯誤: {ex.ToString()}", LogLevel.Error);
                    }
                }));
            }
            await Task.WhenAll(tasks);
        }

        private async Task UploadToGoogleDrive(string zipfilePath, string fileName, string authToken, string folderId)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    // 設定 Authorization 標頭
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

                    // 建立檔案元數據
                    var metadata = new
                    {
                        name = fileName,
                        parents = new[] { folderId }
                    };

                    // 建立 Multipart 請求
                    using (var content = new MultipartFormDataContent())
                    {
                        // 添加元數據
                        var metadataContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(metadata));
                        metadataContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                        content.Add(metadataContent, "metadata");
                        // 添加檔案內容
                        var fileContent = new StreamContent(new FileStream(zipfilePath, FileMode.Open));
                        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/zip");
                        content.Add(fileContent, "file");

                        // 發送請求
                        var response = await httpClient.PostAsync("https://www.googleapis.com/upload/drive/v3/files?uploadType=multipart", content);

                        if (response.IsSuccessStatusCode)
                        {
                            var responseBody = await response.Content.ReadAsStringAsync();
                            Console.WriteLine($"檔案已成功上傳: {responseBody}");
                        }
                        else
                        {
                            var errorContent = await response.Content.ReadAsStringAsync();
                            Console.WriteLine($"檔案上傳失敗: {errorContent}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.ApiLog($"上傳到 Google Drive 發生錯誤: {ex.ToString()}", LogLevel.Error);
            }
        }

    }
}
