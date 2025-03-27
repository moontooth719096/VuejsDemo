using DemoProgressBarAPI.Interfaces;
using DemoProgressBarAPI.Models.YoutubeDonload;
using DemoProgressBarAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DemoProgressBarAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class YoutubeDownloadController : ControllerBase
    {
        private readonly IYoutubeListDownloadService _youtubeservice;
        private readonly LoggingService _loggingService;

        public YoutubeDownloadController(IYoutubeListDownloadService youtubeservice, LoggingService loggingService)
        {
            _youtubeservice = youtubeservice;
            _loggingService = loggingService;
        }

        [HttpGet]
        public IActionResult VideoGet(string VideoID)
        {
            _loggingService.Log("YoutubeDownloadController VideoGet method called.");
            return Ok(_youtubeservice.VideoGet(VideoID));
        }

        [HttpGet]
        public IActionResult PlayListGet(string PlaylistId)
        {
            _loggingService.Log("YoutubeDownloadController PlayListGet method called.");
            return Ok(_youtubeservice.PlayListGet(PlaylistId));
        }

        [HttpPost]
        public async Task<IActionResult> Download(DownloadModel downloadData)
        {
            try
            {
                string connectionid = User?.FindFirst("UserID")?.Value;
                downloadData.ConnectionId = connectionid;
                await _youtubeservice.DownloadApp(downloadData);

                _loggingService.Log("YoutubeDownloadController Download method called.");

                return Ok();
            }
            catch (Exception ex)
            {
                _loggingService.Log($"YoutubeDownloadController Download method failed: {ex.Message}");
                return BadRequest();
            }
        }
    }
}
