using DemoProgressBarAPI.Models.YoutubeDonload;
using Microsoft.AspNetCore.Mvc;

namespace DemoProgressBarAPI.Interfaces
{
    public interface IYoutubeListDownloadService
    {
        IAsyncEnumerable<YotubeDownloadListViewModel> PlayListGet(string PlaylistId);
        Task<IActionResult> DownloadApp(IEnumerable<SelectDataModel> SelectData);
    }
}