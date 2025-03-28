using DemoProgressBarAPI.Interfaces;
using DemoProgressBarAPI.Models.YoutubeDonload;
using DemoProgressBarAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
            return Ok(_youtubeservice.VideoGet(VideoID));
        }

        [HttpGet]
        public IActionResult PlayListGet(string PlaylistId)
        {
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

                return Ok();
            }
            catch (Exception ex)
            {
                _loggingService.ApiLog($"YoutubeDownloadController Download method failed: {ex.Message}",LogLevel.Error);
                return BadRequest();
            }
        }
    }
}
