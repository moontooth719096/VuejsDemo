using DemoProgressBarAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DemoProgressBarAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class LinePayController : Controller
    {
        private readonly LinePayService _linepayservice;
        public LinePayController(LinePayService linepayservice)
        {
            _linepayservice = linepayservice;
        }
        [HttpGet(Name = "GetPayRequest")]
        public async Task<IActionResult> GetPayRequest()
        {
            string orderid = Random.Shared.Next(1000000, 9999999).ToString();
            return Ok(await _linepayservice.RequestPaymentAsync(100, "TWD", orderid, string.Empty));
        }

        [HttpPost(Name = "GetConfirm")]
        public async Task<IActionResult> GetConfirm(string transactionId)
        {
            string orderid = Random.Shared.Next(1000000, 9999999).ToString();
            return Ok(await _linepayservice.ConfirmAsync(100, "TWD", transactionId));
        }
    }
}
