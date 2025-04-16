using DemoProgressBarAPI.Interfaces;
using DemoProgressBarAPI.Models.Discord;
using DemoProgressBarAPI.Models.YoutubeDonload;
using DemoProgressBarAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DemoProgressBarAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiscordController : Controller
    {
        private readonly DiscordService _discordService;

        public DiscordController(DiscordService discordService)
        {
            _discordService = discordService;
        }
        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage(SendMessageModel senddata)
        {
            try
            {
                // 發送訊息到 Discord 頻道
                await _discordService.SendMessageAsync(senddata.ChennelID, senddata.Message);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
    }
}
