using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Checking_stocks_using_GPT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StocksController : ControllerBase
    {
        private readonly AlphaVantageService _alphaVantageService;

        public StocksController(AlphaVantageService alphaVantageService)
        {
            _alphaVantageService = alphaVantageService;
        }

        [HttpGet("GetStockData")]
        public async Task<IActionResult> GetStockData(string symbol)
        {
            try
            {
                var data = await _alphaVantageService.GetStockDataAsync(symbol);
                return Ok(data);
            }
            catch (Exception ex)
            {
                // Log the exception details
                return StatusCode(500, "Stocks Controller Server Error: " + ex.Message);
            }
        }
    }
}
