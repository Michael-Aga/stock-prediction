using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Checking_stocks_using_GPT.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Gpt3Controller : ControllerBase
    {
        private readonly Gpt3Service _gpt3Service;
        private readonly AlphaVantageService _alphaVantageService;
        private readonly MongoDBService _mongoDbService; // Add MongoDB Service
        private readonly ILogger<Gpt3Controller> _logger;

        public Gpt3Controller(Gpt3Service gpt3Service, AlphaVantageService alphaVantageService, MongoDBService mongoDbService, ILogger<Gpt3Controller> logger)
        {
            _gpt3Service = gpt3Service;
            _alphaVantageService = alphaVantageService;
            _mongoDbService = mongoDbService; // Initialize MongoDB Service
            _logger = logger;
        }

        [HttpGet("PredictStockPerformance")]
        public async Task<IActionResult> PredictStockPerformance(string stockSymbol)
        {
            try
            {
                // Fetch stock data from AlphaVantage
                var stockDataJson = await _alphaVantageService.GetStockDataAsync(stockSymbol);

                // Log the data for debugging
                _logger.LogInformation($"Received stock data: {stockDataJson}");

                // Pass the stock data to GPT-3 for prediction
                var gpt3ResponseJson = await _gpt3Service.GetStockPrediction(stockDataJson);

                // Parse the GPT-3 JSON response to extract the prediction text
                var gpt3Response = JObject.Parse(gpt3ResponseJson);
                var predictionText = gpt3Response["choices"]?.FirstOrDefault()?["text"]?.ToString().Trim();

                // Create a new StockPrediction and save it to MongoDB
                var prediction = new StockPrediction
                {
                    StockSymbol = stockSymbol,
                    Prediction = predictionText, // Save only the prediction text
                    Date = DateTime.UtcNow
                };
                await _mongoDbService.CreateAsync(prediction);

                return Ok(gpt3Response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GPT Controller Server Error: {ex.Message}");
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }
    }
}