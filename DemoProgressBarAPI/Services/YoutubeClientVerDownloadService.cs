using DemoProgressBarAPI.Hubs;
using DemoProgressBarAPI.Interfaces;
using DemoProgressBarAPI.Models.YoutubeDonload;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NAudio.Wave;
using NReco.VideoConverter;
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
        public YoutubeClientVerDownloadService(IHubContext<YoutubeDownloadProgressHub> youtubeDownloadProgressHub)
        {
            _youtubeDownloadProgressHub = youtubeDownloadProgressHub;
            _youtubeclient = new YoutubeClient();
        }

        public async IAsyncEnumerable<YotubeDownloadListViewModel> PlayListGet(string PlaylistId)
        {
            //取得youtube清單
            IEnumerable<PlaylistVideo> MusicList = await SearchListYoutubeClientVer_Get(PlaylistId);
            if (MusicList != null)
            {
                using (var enumerator = MusicList.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        PlaylistVideo value = enumerator.Current;
                        yield return new YotubeDownloadListViewModel
                        {
                            IsCheck = true,
                            Title = value.Title,
                            Id = value.Id,
                            ThumbnailUrl = value.Thumbnails.SingleOrDefault(Thumbnail => Thumbnail.Resolution.Area == value.Thumbnails.Max(Thumbnail => Thumbnail.Resolution.Area)).Url,
                            PlayTime = value.Duration.ToString()
                        };
                    }
                }
            }
        }
        public async Task<IActionResult> DownloadApp(IEnumerable<SelectDataModel> SelectData)
        {
            APIResponseModel result = new APIResponseModel { Code = 1 };
            List<Task> downloadList = new List<Task>();
            List<MP4Streaminfo> Vedios = new List<MP4Streaminfo>();
            List<SelectDataModel> Dolist = SelectData.OrderBy(x => x.id).ToList();
            string Token = DateTime.Now.ToString("yyyyMMddHHmmssffff");
            string folderPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "YoutubeDonload", Token);

            // 检查文件夹是否存在
            FileCheck(folderPath);

            int Takecount = 5;
            double TotalCount = Dolist.Count();
            string message = $"音樂下載中";
            double percentage = 0;
            
            while (Dolist.Count() > 0)
            {
                IEnumerable<SelectDataModel> nowlist = Dolist.Take(Takecount);
                Action updateProgress = async () =>
                {
                    percentage = 100 - (Math.Round((Dolist.Count() / TotalCount) * 100));
                    await _youtubeDownloadProgressHub.Clients.All.SendAsync("YoutubeDownloadProgress", message, percentage);
                };
                foreach (var searchResult in nowlist)
                {
                    
                    string filename = $"{searchResult.Title}.mp4";
                    
                    //移除windows不允許的檔名字元
                    filename = Regex.Replace(filename, "[\\/:*?\"<>|]", "");
                    // 保存 MP3 文件
                    //var outPath = Path.Combine(folderPath, filename);
                    filename = filename.Replace(" ", "");

                    downloadList.Add(Task.Run(async () => {
                        MP4Streaminfo vedioinfo = await VedioStream_Get(searchResult.VideoId, folderPath, filename);
                        if(vedioinfo!=null)
                            Vedios.Add(vedioinfo);
                        updateProgress();
                    }));

                }
                await Task.WhenAll(downloadList);
                Dolist.RemoveRange(0, nowlist.Count());
                //downloadList.EX.Exception.InnerExceptions.Select(o => o.Message).ToArray());

            }

            percentage = 100;
            await _youtubeDownloadProgressHub.Clients.All.SendAsync("YoutubeDownloadProgress", message, percentage);

            //轉換mp4toMP3
            await ConvertToMP3(Vedios);

            return await ZipDownloadFile(Token, folderPath);
        }
        private async Task<IActionResult> ZipDownloadFile(string Token, string tragePath)
        {
            string zipFileName = $"compressed-files-{Token}.zip";
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


            }
          

            // 构建响应，将zip文件提供给客户端下载
            var memoryStream = new MemoryStream();
            using (var fs = new FileStream(AllzipFilePath, FileMode.Open))
            {
                await fs.CopyToAsync(memoryStream);
            }
            memoryStream.Seek(0, SeekOrigin.Begin);

            return new FileStreamResult(memoryStream, System.Net.Mime.MediaTypeNames.Application.Octet);

        }

        private async Task<IEnumerable<PlaylistVideo>> SearchListYoutubeClientVer_Get(string PlaylistId)
        {
            var playlistUrl = "https://youtube.com/playlist?list=" + PlaylistId;

            return await _youtubeclient.Playlists.GetVideosAsync(playlistUrl);
        }
        private async Task<MP4Streaminfo> VedioStream_Get(VideoId item, string folderPath, string filename)
        {
            try
            {
                // 解析影片信息
                var video = await _youtubeclient.Videos.Streams.GetManifestAsync(item);

                // 擷取聲音(取最高音質
                var audioStreamInfo = video.GetAudioOnlyStreams().GetWithHighestBitrate();
                //var audioStreamInfo = video.GetAudioStreams().GetWithHighestBitrate();
                //var audioStreamInfo = video.GetAudioOnlyStreams().Where(x=>x.Container == Container.Mp4).GetWithHighestBitrate();

                //設定要轉換的kbps
                int nowkbps = kbpsSet(audioStreamInfo.Bitrate.KiloBitsPerSecond);


                //取得stream
                var audioStream = await _youtubeclient.Videos.Streams.GetAsync(audioStreamInfo);
                //string  MP4outPath = Path.Combine(folderPath, filename);
                //outPath = outPath.Replace(".mp3", ".mp4");
                //下載
                //await _youtubeclient.Videos.Streams.DownloadAsync(audioStreamInfo, MP4outPath);
                
                
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
                Console.WriteLine($"下載 {filename} 發生錯誤");
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

        private async Task ConvertToMP3(List<MP4Streaminfo> vodeos)
        {
            string message = "格式轉換中";
            int count = 0;
            double TotalCount = vodeos.Count();
            double percentage = 0;
            //var Convert = new NReco.VideoConverter.FFMpegConverter();
            List<Task> tasks = new List<Task>();
            foreach (MP4Streaminfo vedio in vodeos)
            {
                var settings = new ConvertSettings
                {
                    AudioCodec = "mp3",
                    CustomOutputArgs = $"-b:a {vedio.Kbps}k"
                };

                try
                {
                    tasks.Add(Task.Run(async () => {
                        var Convert = new NReco.VideoConverter.FFMpegConverter();
                        var vidtask = Convert.ConvertLiveMedia(vedio.MP4Stream, null, vedio.OutputPath, null, settings);
                        vidtask.Start();
                        vidtask.Wait();
                        count++;
                        percentage = Math.Round((count / TotalCount) * 100);
               
                        await _youtubeDownloadProgressHub.Clients.All.SendAsync("YoutubeDownloadProgress", message, percentage);
                        await Console.Out.WriteLineAsync($"已保存 MP3 文件至 {vedio.OutputPath}。");
                    }));
                    await Task.WhenAll(tasks);
                    //Convert.ConvertMedia(MP4outPath, null, MP3outPath, null, settings);

                }
                catch (Exception ex)
                {

                }
                percentage = 100;
                await _youtubeDownloadProgressHub.Clients.All.SendAsync("YoutubeDownloadProgress", message, percentage);
            }
        }
         
    }
}
