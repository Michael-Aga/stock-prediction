namespace Checking_stocks_using_GPT
{
    public class AlphaVantageService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public AlphaVantageService(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
        }

        public async Task<string> GetStockDataAsync(string symbol)
        {
            var url = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={_apiKey}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}