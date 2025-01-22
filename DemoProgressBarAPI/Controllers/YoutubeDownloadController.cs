using DemoProgressBarAPI.Interfaces;
using DemoProgressBarAPI.Models.YoutubeDonload;
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
       
        public YoutubeDownloadController(IYoutubeListDownloadService youtubeservice)
        {
            _youtubeservice = youtubeservice;
        }
        [HttpGet]
        public IActionResult VideoGet(string VideoID)
        {
          return  Ok(_youtubeservice.VideoGet(VideoID));
        }
        [HttpGet]
        public IActionResult PlayListGet(string PlaylistId)
        {
          return  Ok(_youtubeservice.PlayListGet(PlaylistId));
        }

        [HttpPost]
        public async Task<IActionResult> Download(IEnumerable<SelectDataModel> SelectData)
        {
            try
            {
                return await _youtubeservice.DownloadApp(SelectData);
            }
            catch(Exception ex)
            {
                return Ok();
            }
        }
    }
}
