using InvestmentSimulator.Domain.Common;

namespace InvestmentSimulator.Domain.Tests.Common;

public class MonetaryPrecisionTests
{
    [Fact]
    public void IntermediateDecimalPlaces_ShouldBeAtLeastEight()
    {
        Assert.True(MonetaryPrecision.IntermediateDecimalPlaces >= 8);
    }

    [Theory]
    [InlineData(nameof(MonetaryPrecision.CurrencyDecimalPlaces), 2)]
    [InlineData(nameof(MonetaryPrecision.PercentageDecimalPlaces), 4)]
    public void PresentationDecimalPlaces_ShouldMatchErsSection28(string propertyName, int expected)
    {
        var actual = propertyName switch
        {
            nameof(MonetaryPrecision.CurrencyDecimalPlaces) => MonetaryPrecision.CurrencyDecimalPlaces,
            nameof(MonetaryPrecision.PercentageDecimalPlaces) => MonetaryPrecision.PercentageDecimalPlaces,
            _ => throw new ArgumentOutOfRangeException(nameof(propertyName)),
        };

        Assert.Equal(expected, actual);
    }
}
