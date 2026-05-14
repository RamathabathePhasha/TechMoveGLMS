using System.Text.Json;

namespace TechMoveGLMS.Services
{
    // This class actually does the currency conversion work
    public class CurrencyService : ICurrencyService
    {
        // To make internet requests to the API
        private readonly HttpClient _httpClient;

        // To log errors if something goes wrong
        private readonly ILogger<CurrencyService> _logger;

        // FREE API URL - no API key needed!
        // This website gives us live exchange rates
        private const string EXCHANGE_API_URL = "https://api.exchangerate-api.com/v4/latest/USD";

        // Constructor - runs when the service is created
        public CurrencyService(HttpClient httpClient, ILogger<CurrencyService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // This method goes to the internet to get the current exchange rate
        public async Task<decimal> GetUsdToZarRateAsync()
        {
            try
            {
                _logger.LogInformation("Getting current exchange rate from API...");

                // 1. Call the API (wait for response)
                var response = await _httpClient.GetAsync(EXCHANGE_API_URL);

                // 2. Make sure it worked
                response.EnsureSuccessStatusCode();

                // 3. Read the JSON response as text
                var jsonContent = await response.Content.ReadAsStringAsync();

                // 4. Parse the JSON to find the ZAR rate
                var jsonDocument = JsonDocument.Parse(jsonContent);
                var root = jsonDocument.RootElement;

                // 5. Extract the ZAR rate from the JSON
                if (root.TryGetProperty("rates", out var ratesElement))
                {
                    if (ratesElement.TryGetProperty("ZAR", out var zarRateElement))
                    {
                        var rate = zarRateElement.GetDecimal();
                        _logger.LogInformation($"Current rate: 1 USD = {rate} ZAR");
                        return rate;
                    }
                }

                // If something went wrong, use a reasonable default rate
                _logger.LogWarning("Could not get rate, using default 19.50");
                return 19.50m;
            }
            catch (Exception ex)
            {
                // If API fails, log error and return default rate
                _logger.LogError(ex, "Failed to get exchange rate");
                return 19.50m; // Fallback rate so app doesn't break
            }
        }

        // This method does the actual math: USD × Rate = ZAR
        public decimal ConvertUsdToZar(decimal usdAmount, decimal exchangeRate)
        {
            // Protect against bad input
            if (usdAmount < 0)
                throw new ArgumentException("Amount cannot be negative");

            if (exchangeRate <= 0)
                throw new ArgumentException("Exchange rate must be positive");

            // Do the conversion and round to 2 decimal places
            return Math.Round(usdAmount * exchangeRate, 2);
        }
    }
}