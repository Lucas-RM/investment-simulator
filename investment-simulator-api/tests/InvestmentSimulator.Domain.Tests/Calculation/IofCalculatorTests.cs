using InvestmentSimulator.Domain.Calculation;
using InvestmentSimulator.Domain.Common;
using InvestmentSimulator.Domain.Exceptions;

namespace InvestmentSimulator.Domain.Tests.Calculation;

public class IofCalculatorTests
{
    [Theory]
    [InlineData(0, 0.00)]
    [InlineData(1, 0.96)]
    [InlineData(2, 0.93)]
    [InlineData(3, 0.90)]
    [InlineData(4, 0.86)]
    [InlineData(5, 0.83)]
    [InlineData(6, 0.80)]
    [InlineData(7, 0.76)]
    [InlineData(8, 0.73)]
    [InlineData(9, 0.70)]
    [InlineData(10, 0.66)]
    [InlineData(11, 0.63)]
    [InlineData(12, 0.60)]
    [InlineData(13, 0.56)]
    [InlineData(14, 0.53)]
    [InlineData(15, 0.50)]
    [InlineData(16, 0.46)]
    [InlineData(17, 0.43)]
    [InlineData(18, 0.40)]
    [InlineData(19, 0.36)]
    [InlineData(20, 0.33)]
    [InlineData(21, 0.30)]
    [InlineData(22, 0.26)]
    [InlineData(23, 0.23)]
    [InlineData(24, 0.20)]
    [InlineData(25, 0.16)]
    [InlineData(26, 0.13)]
    [InlineData(27, 0.10)]
    [InlineData(28, 0.06)]
    [InlineData(29, 0.03)]
    [InlineData(30, 0.00)]
    [InlineData(31, 0.00)]
    [InlineData(180, 0.00)]
    public void GetRate_ShouldFollowRegressiveTable(int daysInvested, decimal expectedRate)
    {
        Assert.Equal(expectedRate, IofCalculator.GetRate(daysInvested));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(29, false)]
    [InlineData(30, true)]
    [InlineData(31, true)]
    [InlineData(720, true)]
    public void IsExempt_ShouldBeTrueFromThirtyDaysOnward(int daysInvested, bool expected)
    {
        Assert.Equal(expected, IofCalculator.IsExempt(daysInvested));
    }

    [Fact]
    public void Calculate_ShouldApplyOnlyOnYield()
    {
        const decimal yield = 100m;
        const int days = 15; // 50%

        var result = IofCalculator.Calculate(yield, days);

        Assert.Equal(50m, result);
    }

    [Fact]
    public void Calculate_OnDayOne_ShouldUseNinetySixPercent()
    {
        const decimal yield = 250m;

        var result = IofCalculator.Calculate(yield, 1);

        var expected = Math.Round(
            yield * 0.96m,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Calculate_OnDayTwentyNine_ShouldUseThreePercent()
    {
        const decimal yield = 1_000m;

        var result = IofCalculator.Calculate(yield, 29);

        Assert.Equal(30m, result);
    }

    [Theory]
    [InlineData(30)]
    [InlineData(31)]
    [InlineData(360)]
    public void Calculate_WhenDaysAtOrAboveThirty_ShouldBeZero(int daysInvested)
    {
        Assert.Equal(0m, IofCalculator.Calculate(1_000m, daysInvested));
    }

    [Fact]
    public void Calculate_WithZeroYield_ShouldBeZero()
    {
        Assert.Equal(0m, IofCalculator.Calculate(0m, 10));
    }

    [Fact]
    public void Calculate_WithZeroDays_ShouldBeZero()
    {
        Assert.Equal(0m, IofCalculator.Calculate(100m, 0));
    }

    [Fact]
    public void Calculate_ShouldRoundToIntermediatePrecision()
    {
        // 10.123456789 × 0.50 = 5.0617283945 → 8 d.p. AwayFromZero → 5.06172839
        const decimal yield = 10.123456789m;

        var result = IofCalculator.Calculate(yield, 15);

        var expected = Math.Round(
            yield * 0.50m,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Calculate_ShouldRejectNegativeYield()
    {
        Assert.Throws<DomainValidationException>(() =>
            IofCalculator.Calculate(-1m, 10));
    }

    [Fact]
    public void Calculate_ShouldRejectNegativeDays()
    {
        Assert.Throws<DomainValidationException>(() =>
            IofCalculator.Calculate(100m, -1));
    }

    [Fact]
    public void GetRate_ShouldRejectNegativeDays()
    {
        Assert.Throws<DomainValidationException>(() =>
            IofCalculator.GetRate(-1));
    }

    [Fact]
    public void IsExempt_ShouldRejectNegativeDays()
    {
        Assert.Throws<DomainValidationException>(() =>
            IofCalculator.IsExempt(-1));
    }

    [Fact]
    public void ExemptionDays_ShouldBeThirty()
    {
        Assert.Equal(30, IofCalculator.ExemptionDays);
    }
}
