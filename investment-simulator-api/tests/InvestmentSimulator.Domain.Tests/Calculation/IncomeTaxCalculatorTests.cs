using InvestmentSimulator.Domain.Calculation;
using InvestmentSimulator.Domain.Common;
using InvestmentSimulator.Domain.Exceptions;

namespace InvestmentSimulator.Domain.Tests.Calculation;

public class IncomeTaxCalculatorTests
{
    [Theory]
    [InlineData(0, 0.225)]
    [InlineData(1, 0.225)]
    [InlineData(180, 0.225)]
    [InlineData(181, 0.20)]
    [InlineData(360, 0.20)]
    [InlineData(361, 0.175)]
    [InlineData(720, 0.175)]
    [InlineData(721, 0.15)]
    [InlineData(1000, 0.15)]
    public void GetRate_ShouldFollowRegressiveTable(int daysInvested, decimal expectedRate)
    {
        Assert.Equal(expectedRate, IncomeTaxCalculator.GetRate(daysInvested));
    }

    [Fact]
    public void Calculate_UpTo180Days_ShouldUseTwentyTwoPointFivePercent()
    {
        const decimal yield = 1_000m;

        var result = IncomeTaxCalculator.Calculate(yield, 180);

        Assert.Equal(225m, result);
    }

    [Fact]
    public void Calculate_From181To360Days_ShouldUseTwentyPercent()
    {
        const decimal yield = 1_000m;

        var result = IncomeTaxCalculator.Calculate(yield, 181);

        Assert.Equal(200m, result);
    }

    [Fact]
    public void Calculate_From361To720Days_ShouldUseSeventeenPointFivePercent()
    {
        const decimal yield = 1_000m;

        var result = IncomeTaxCalculator.Calculate(yield, 361);

        Assert.Equal(175m, result);
    }

    [Fact]
    public void Calculate_Above720Days_ShouldUseFifteenPercent()
    {
        const decimal yield = 1_000m;

        var result = IncomeTaxCalculator.Calculate(yield, 721);

        Assert.Equal(150m, result);
    }

    [Fact]
    public void Calculate_ShouldApplyOnlyOnYield()
    {
        // Principal is never taxed — only the yield passed in.
        const decimal yield = 250m;
        const int days = 100; // 22.5%

        var result = IncomeTaxCalculator.Calculate(yield, days);

        Assert.Equal(56.25m, result);
    }

    [Fact]
    public void Calculate_WithZeroYield_ShouldBeZero()
    {
        Assert.Equal(0m, IncomeTaxCalculator.Calculate(0m, 100));
    }

    [Fact]
    public void Calculate_ShouldRoundToIntermediatePrecision()
    {
        // 10.123456789 × 0.225 = 2.277777777525 → 8 d.p. AwayFromZero → 2.27777778
        const decimal yield = 10.123456789m;

        var result = IncomeTaxCalculator.Calculate(yield, 90);

        var expected = Math.Round(
            yield * 0.225m,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Calculate_ShouldRejectNegativeYield()
    {
        Assert.Throws<DomainValidationException>(() =>
            IncomeTaxCalculator.Calculate(-1m, 100));
    }

    [Fact]
    public void Calculate_ShouldRejectNegativeDays()
    {
        Assert.Throws<DomainValidationException>(() =>
            IncomeTaxCalculator.Calculate(100m, -1));
    }

    [Fact]
    public void GetRate_ShouldRejectNegativeDays()
    {
        Assert.Throws<DomainValidationException>(() =>
            IncomeTaxCalculator.GetRate(-1));
    }

    [Fact]
    public void RateConstants_ShouldMatchOfficialTable()
    {
        Assert.Equal(0.225m, IncomeTaxCalculator.RateUpTo180Days);
        Assert.Equal(0.20m, IncomeTaxCalculator.RateFrom181To360Days);
        Assert.Equal(0.175m, IncomeTaxCalculator.RateFrom361To720Days);
        Assert.Equal(0.15m, IncomeTaxCalculator.RateAbove720Days);
    }

    [Theory]
    [InlineData(180, 181)]
    [InlineData(360, 361)]
    [InlineData(720, 721)]
    public void GetRate_ShouldChangeExactlyAtBracketBoundaries(int lastDayOfBracket, int firstDayOfNextBracket)
    {
        var rateBefore = IncomeTaxCalculator.GetRate(lastDayOfBracket);
        var rateAfter = IncomeTaxCalculator.GetRate(firstDayOfNextBracket);

        Assert.True(rateBefore > rateAfter);
    }
}
