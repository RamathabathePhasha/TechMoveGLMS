namespace TechMoveGLMS.Services
{
    // This is an interface - it defines WHAT the service can do
    public interface ICurrencyService
    {
        // Gets the current USD to ZAR exchange rate from internet
        Task<decimal> GetUsdToZarRateAsync();

        // Converts USD amount to ZAR using a given exchange rate
        decimal ConvertUsdToZar(decimal usdAmount, decimal exchangeRate);
    }
}