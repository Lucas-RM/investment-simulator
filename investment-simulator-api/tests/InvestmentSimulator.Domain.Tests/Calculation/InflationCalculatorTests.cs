using InvestmentSimulator.Domain.Calculation;
using InvestmentSimulator.Domain.Common;
using InvestmentSimulator.Domain.Entities;
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

    [Fact]
    public void CalculateAccumulatedInflationForPeriod_WithPartialYear_ShouldProrateAnnualRate()
    {
        // Auditor scenario: 2026-07-06 → 2026-12-31 = 178 calendar days; IPCA 4.10% a.a.
        var start = new DateOnly(2026, 7, 6);
        var end = new DateOnly(2026, 12, 31);
        var rates = new[] { new AnnualRate(2026, 0.041m) };

        var result = InflationCalculator.CalculateAccumulatedInflationForPeriod(start, end, rates);

        var days = end.DayNumber - start.DayNumber;
        Assert.Equal(178, days);

        var expected = Math.Round(
            (decimal)Math.Pow(1.041, days / 365.0) - 1m,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);

        Assert.Equal(expected, result);
        Assert.True(result > 0m);
        Assert.True(result < 0.041m);
    }

    [Fact]
    public void CalculateAccumulatedInflationForPeriod_WithFullYears_ShouldMatchCompoundProduct()
    {
        // [2026-01-01, 2029-01-01) covers three full years.
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2029, 1, 1);
        var rates = new[]
        {
            new AnnualRate(2026, 0.05m),
            new AnnualRate(2027, 0.04m),
            new AnnualRate(2028, 0.045m),
        };

        var result = InflationCalculator.CalculateAccumulatedInflationForPeriod(start, end, rates);

        Assert.Equal(
            InflationCalculator.CalculateAccumulatedInflation([0.05m, 0.04m, 0.045m]),
            result);
    }

    [Fact]
    public void CalculateInflationAdjustedAmount_ForPeriod_ShouldUseProratedInflation()
    {
        const decimal netAmount = 5_588.36664083m;
        var start = new DateOnly(2026, 7, 6);
        var end = new DateOnly(2026, 12, 31);
        var rates = new[] { new AnnualRate(2026, 0.041m) };

        var result = InflationCalculator.CalculateInflationAdjustedAmount(
            netAmount,
            start,
            end,
            rates);

        var accumulated = InflationCalculator.CalculateAccumulatedInflationForPeriod(
            start,
            end,
            rates);
        var expected = InflationCalculator.AdjustForPurchasingPower(netAmount, accumulated);

        Assert.Equal(expected, result);
        // Full-year 4.10% would yield ~5368; pro-rata must be higher (less inflation).
        var fullYearAdjusted = InflationCalculator.CalculateInflationAdjustedAmount(
            netAmount,
            [0.041m]);
        Assert.True(result > fullYearAdjusted);
        Assert.InRange(result, 5_470m, 5_490m);
    }

    [Fact]
    public void CalculateAccumulatedInflationForPeriod_ShouldRejectMissingYearRate()
    {
        Assert.Throws<DomainValidationException>(() =>
            InflationCalculator.CalculateAccumulatedInflationForPeriod(
                new DateOnly(2026, 1, 1),
                new DateOnly(2027, 6, 1),
                [new AnnualRate(2026, 0.04m)]));
    }
}
