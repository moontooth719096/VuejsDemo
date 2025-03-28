using DemoProgressBarAPI.Models.YoutubeDonload;
using Microsoft.AspNetCore.Mvc;

namespace DemoProgressBarAPI.Interfaces
{
    public interface IYoutubeListDownloadService
    {
        IAsyncEnumerable<YotubeDownloadListViewModel> PlayListGet(string PlaylistId);

        IAsyncEnumerable<YotubeDownloadListViewModel> VideoGet(string VideoID);
        Task<string> DownloadApp(DownloadModel downloadData);
    }
}