using Xunit;

namespace TechMoveGLMS.Tests
{
    public class CurrencyServiceTests
    {
        // TEST 1: Basic multiplication
        [Fact]
        public void ConvertUsdToZar_When100UsdAtRate19_Returns1900Zar()
        {
            decimal usd = 100;
            decimal rate = 19;
            decimal expected = 1900;
            decimal actual = usd * rate;
            Assert.Equal(expected, actual);
        }

        // TEST 2: Different numbers
        [Fact]
        public void ConvertUsdToZar_When50UsdAtRate20_Returns1000Zar()
        {
            decimal usd = 50;
            decimal rate = 20;
            decimal expected = 1000;
            decimal actual = usd * rate;
            Assert.Equal(expected, actual);
        }

        // TEST 3: Zero test
        [Fact]
        public void ConvertUsdToZar_WhenZeroUsd_ReturnsZero()
        {
            decimal usd = 0;
            decimal rate = 19.50m;
            decimal expected = 0;
            decimal actual = usd * rate;
            Assert.Equal(expected, actual);
        }

        // TEST 4: One USD test
        [Fact]
        public void ConvertUsdToZar_WhenOneUsdAtRate18_Returns18Zar()
        {
            decimal usd = 1;
            decimal rate = 18;
            decimal expected = 18;
            decimal actual = usd * rate;
            Assert.Equal(expected, actual);
        }

        // TEST 5: Rounding test with clean numbers
        [Fact]
        public void ConvertUsdToZar_WhenAmountNeedsRounding_RoundsToTwoDecimals()
        {
            //e.g 10.55 * 15.75 = 166.1625, rounds to 166.16
            decimal usd = 10.55m;
            decimal rate = 15.75m;

            decimal rawResult = usd * rate;
            decimal roundedResult = Math.Round(rawResult, 2);

            decimal expected = 166.16m;

            Assert.Equal(expected, roundedResult);
        }

        // TEST 6: Negative numbers
        [Fact]
        public void ConvertUsdToZar_WithNegativeUsd_ReturnsNegative()
        {
            decimal usd = -100;
            decimal rate = 19.50m;
            decimal expected = -1950;
            decimal actual = usd * rate;
            Assert.Equal(expected, actual);
        }
    }
}