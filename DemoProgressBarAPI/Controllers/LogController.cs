using DemoProgressBarAPI.Models.Log;
using DemoProgressBarAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost("write")]
        public IActionResult WriteLog([FromBody] LogRequest logRequest)
        {
            _loggingService.Log(logRequest.Message, logRequest.CallEnd, logRequest.LogLevel);
            return Ok();
        }
    }
}
