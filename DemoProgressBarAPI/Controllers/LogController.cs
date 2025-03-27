using DemoProgressBarAPI.Services;
using DemoProgressBarAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace DemoProgressBarAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogController : ControllerBase
    {
        private readonly LoggingService _loggingService;

        public LogController(LoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        [Authorize(Policy = "RequireUserLevel99")]
        [HttpGet("read")]
        public async Task<IActionResult> ReadLog()
        {
            var logEntries = await _loggingService.ReadLogAsync();
            return Ok(logEntries);
        }
    }
}
