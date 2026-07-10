using InvestmentSimulator.Domain.Calculation;
using InvestmentSimulator.Domain.Common;
using InvestmentSimulator.Domain.Exceptions;

namespace InvestmentSimulator.Domain.Tests.Calculation;

public class InflationCalculatorTests
{
    [Fact]
    public void CalculateAccumulatedInflation_WithErsExample_ShouldMatchCompoundProduct()
    {
        // ERS §17: (1.05 × 1.04 × 1.045) − 1
        decimal[] rates = [0.05m, 0.04m, 0.045m];

        var result = InflationCalculator.CalculateAccumulatedInflation(rates);

        var expected = Math.Round(
            (1.05m * 1.04m * 1.045m) - 1m,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);

        Assert.Equal(expected, result);
        Assert.Equal(0.14114m, result);
    }

    [Fact]
    public void CalculateAccumulatedInflation_WithSingleRate_ShouldEqualThatRate()
    {
        var result = InflationCalculator.CalculateAccumulatedInflation([0.05m]);

        Assert.Equal(0.05m, result);
    }

    [Fact]
    public void CalculateAccumulatedInflation_WithEmptySequence_ShouldBeZero()
    {
        Assert.Equal(0m, InflationCalculator.CalculateAccumulatedInflation([]));
    }

    [Fact]
    public void CalculateAccumulatedInflation_WithZeroRates_ShouldBeZero()
    {
        Assert.Equal(0m, InflationCalculator.CalculateAccumulatedInflation([0m, 0m, 0m]));
    }

    [Fact]
    public void CalculateAccumulatedInflation_ShouldRoundToIntermediatePrecision()
    {
        // Product may need rounding beyond 8 d.p.
        decimal[] rates = [0.0333m, 0.0444m, 0.0555m];

        var result = InflationCalculator.CalculateAccumulatedInflation(rates);

        var expected = Math.Round(
            (1.0333m * 1.0444m * 1.0555m) - 1m,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateAccumulatedInflation_ShouldRejectNegativeRate()
    {
        Assert.Throws<DomainValidationException>(() =>
            InflationCalculator.CalculateAccumulatedInflation([0.05m, -0.01m]));
    }

    [Fact]
    public void CalculateAccumulatedInflation_ShouldRejectNullSequence()
    {
        Assert.Throws<ArgumentNullException>(() =>
            InflationCalculator.CalculateAccumulatedInflation(null!));
    }

    [Fact]
    public void AdjustForPurchasingPower_ShouldDivideNetByOnePlusInflation()
    {
        // 11_411.40 / 1.14114 ≈ 10_000
        const decimal netAmount = 11_411.40m;
        const decimal accumulatedInflation = 0.14114m;

        var result = InflationCalculator.AdjustForPurchasingPower(netAmount, accumulatedInflation);

        var expected = Math.Round(
            netAmount / (1m + accumulatedInflation),
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void AdjustForPurchasingPower_WithZeroInflation_ShouldReturnNetAmount()
    {
        const decimal netAmount = 10_000.123456789m;

        var result = InflationCalculator.AdjustForPurchasingPower(netAmount, 0m);

        var expected = Math.Round(
            netAmount,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void AdjustForPurchasingPower_WithZeroNet_ShouldBeZero()
    {
        Assert.Equal(0m, InflationCalculator.AdjustForPurchasingPower(0m, 0.05m));
    }

    [Fact]
    public void AdjustForPurchasingPower_ShouldRejectNegativeNetAmount()
    {
        Assert.Throws<DomainValidationException>(() =>
            InflationCalculator.AdjustForPurchasingPower(-1m, 0.05m));
    }

    [Fact]
    public void AdjustForPurchasingPower_ShouldRejectAccumulatedInflationOfMinusOneOrLess()
    {
        Assert.Throws<DomainValidationException>(() =>
            InflationCalculator.AdjustForPurchasingPower(1_000m, -1m));

        Assert.Throws<DomainValidationException>(() =>
            InflationCalculator.AdjustForPurchasingPower(1_000m, -1.5m));
    }

    [Fact]
    public void CalculateInflationAdjustedAmount_ShouldComposeAccumulationAndAdjustment()
    {
        const decimal netAmount = 11_411.40m;
        decimal[] rates = [0.05m, 0.04m, 0.045m];

        var result = InflationCalculator.CalculateInflationAdjustedAmount(netAmount, rates);

        var accumulated = InflationCalculator.CalculateAccumulatedInflation(rates);
        var expected = InflationCalculator.AdjustForPurchasingPower(netAmount, accumulated);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateInflationAdjustedAmount_WithZeroInflation_ShouldEqualNetAmount()
    {
        const decimal netAmount = 5_000m;

        var result = InflationCalculator.CalculateInflationAdjustedAmount(netAmount, [0m]);

        Assert.Equal(5_000m, result);
    }
}
