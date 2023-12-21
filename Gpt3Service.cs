using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Checking_stocks_using_GPT
{
    public class Gpt3Service
    {
        private readonly HttpClient _httpClient;

        public Gpt3Service(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient("Gpt3Client");
        }

        public async Task<string> GetStockPrediction(string stockDataJson)
        {
            var stockSymbol = ExtractStockSymbol(stockDataJson);
            var jsonData = JObject.Parse(stockDataJson);
            var timeSeriesData = jsonData["Time Series (Daily)"].ToObject<Dictionary<string, Dictionary<string, string>>>();

            string prompt = $"Analyze the following recent stock data for {stockSymbol} and predict whether the stock value will likely go up or down in the upcoming week:\n\n";

            // Adding recent stock data to the prompt
            foreach (var day in timeSeriesData.Take(30)) // You can adjust the number of days you include
            {
                prompt += $"{day.Key}: Open - {day.Value["1. open"]}, High - {day.Value["2. high"]}, Low - {day.Value["3. low"]}, Close - {day.Value["4. close"]}, Volume - {day.Value["5. volume"]}\n";
            }

            prompt += "\nBased on this data, is it more likely for the stock value of AAPL to go up or down in the upcoming week?";

            var requestData = new
            {
                model = "text-davinci-003", // Specify GPT-3.5 Davinci model
                prompt = prompt,
                max_tokens = 150
            };

            HttpResponseMessage response;
            int retryCount = 0;
            int maxRetries = 3; // Adjust as needed

            do
            {
                response = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/completions", requestData);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return responseContent; // Parse or transform the content as needed
                }

                retryCount++;
                await Task.Delay(2000 * retryCount); // Exponential backoff
            }
            while (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests && retryCount < maxRetries);

            // If all retries fail, log the error and throw an exception
            if (!response.IsSuccessStatusCode)
            {
                // Log the error here (consider using a logging framework)
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error response from OpenAI: {errorContent}");
                throw new HttpRequestException($"Request failed with status code: {response.StatusCode} and message: {errorContent}");
            }

            return null; // Or handle this case appropriately
        }

        private string ExtractStockSymbol(string stockDataJson)
        {
            var jsonData = JObject.Parse(stockDataJson);
            return jsonData["Meta Data"]["2. Symbol"].ToString();
        }

        private string SummarizeRecentStockData(string stockDataJson)
        {
            var jsonData = JObject.Parse(stockDataJson);
            var timeSeries = jsonData["Time Series (Daily)"] as JObject;

            var recentData = timeSeries.Properties()
                                .Take(90)
                                .Select(p => p.Value)
                                .ToList();

            decimal highValue = recentData.Max(x => decimal.Parse(x["2. high"].ToString()));
            decimal lowValue = recentData.Min(x => decimal.Parse(x["3. low"].ToString()));
            decimal latestClose = decimal.Parse(recentData.First()["4. close"].ToString());

            decimal firstClose = decimal.Parse(recentData.Last()["4. close"].ToString());
            string overallTrend = latestClose > firstClose ? "an upward trend" : latestClose < firstClose ? "a downward trend" : "a stable trend";

            string summary = $"an overall {overallTrend}, with a high of {highValue} and a low of {lowValue}, and the latest closing value at {latestClose}.";

            return summary;
        }
    }
}
