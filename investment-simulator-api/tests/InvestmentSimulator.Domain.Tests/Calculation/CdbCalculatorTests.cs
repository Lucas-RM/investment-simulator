using InvestmentSimulator.Domain.Calculation;
using InvestmentSimulator.Domain.Calendar;
using InvestmentSimulator.Domain.Common;
using InvestmentSimulator.Domain.Exceptions;
using InvestmentSimulator.Domain.Rates;

namespace InvestmentSimulator.Domain.Tests.Calculation;

public class CdbCalculatorTests
{
    [Fact]
    public void CalculateDailyYieldRate_ShouldMultiplyDailyCdiByProfitability()
    {
        var dailyCdi = RateConverter.AnnualToDaily(0.15m);
        const decimal profitability = 1.10m;

        var result = CdbCalculator.CalculateDailyYieldRate(dailyCdi, profitability);

        var expected = Math.Round(
            dailyCdi * profitability,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateDailyYieldRate_With100Percent_ShouldEqualDailyCdi()
    {
        var dailyCdi = RateConverter.AnnualToDaily(0.15m);

        var result = CdbCalculator.CalculateDailyYieldRate(dailyCdi, 1.0m);

        Assert.Equal(dailyCdi, result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.1)]
    [InlineData(-1)]
    public void CalculateDailyYieldRate_ShouldRejectNonPositiveProfitability(decimal profitability)
    {
        Assert.Throws<DomainValidationException>(() =>
            CdbCalculator.CalculateDailyYieldRate(0.0005m, profitability));
    }

    [Fact]
    public void CalculateEffectiveAnnualRate_ShouldMatchErsExample()
    {
        // ERS section 12: CDI 15% × 110% = 16.5%
        var result = CdbCalculator.CalculateEffectiveAnnualRate(0.15m, 1.10m);

        Assert.Equal(0.165m, result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CalculateEffectiveAnnualRate_ShouldRejectNonPositiveProfitability(decimal profitability)
    {
        Assert.Throws<DomainValidationException>(() =>
            CdbCalculator.CalculateEffectiveAnnualRate(0.15m, profitability));
    }
}

public class CdbDailyYieldRateProviderTests
{
    private static readonly FinancialCalendar Calendar = new(isHoliday: _ => false);

    [Fact]
    public void Constructor_ShouldRejectNonPositiveProfitability()
    {
        Assert.Throws<DomainValidationException>(() => new CdbDailyYieldRateProvider(0m));
        Assert.Throws<DomainValidationException>(() => new CdbDailyYieldRateProvider(-1m));
    }

    [Fact]
    public void GetDailyYieldRate_ShouldApplyCdiTimesProfitability()
    {
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 12, 31);
        var index = RateSchedule.FromSingleRate(0.15m, start, end);
        var ipca = RateSchedule.FromSingleRate(0.05m, start, end);
        var context = new SimulationRateContext(index, ipca);
        context.AdvanceToYear(2026);

        var provider = new CdbDailyYieldRateProvider(1.10m);
        var result = provider.GetDailyYieldRate(context);

        Assert.Equal(
            CdbCalculator.CalculateDailyYieldRate(context.CurrentIndexDailyRate, 1.10m),
            result);
    }

    [Fact]
    public void GetDailyYieldRate_ShouldRejectNullContext()
    {
        var provider = new CdbDailyYieldRateProvider(1.0m);

        Assert.Throws<ArgumentNullException>(() => provider.GetDailyYieldRate(null!));
    }

    [Fact]
    public void Engine_WithCdbProvider_ShouldAccrueAtCdiTimesProfitability()
    {
        var start = new DateOnly(2026, 1, 2); // Friday
        var end = new DateOnly(2026, 1, 9);   // Friday — business days: 5,6,7,8,9
        var index = RateSchedule.FromSingleRate(0.15m, start, end);
        var ipca = RateSchedule.FromSingleRate(0.05m, start, end);
        var rateContext = new SimulationRateContext(index, ipca);

        const decimal profitability = 1.10m;
        var dailyRate = CdbCalculator.CalculateDailyYieldRate(
            RateConverter.AnnualToDaily(0.15m),
            profitability);

        var positions = new List<ContributionPosition> { new(start, 10_000m) };
        var engine = new DailyCalculationEngine(Calendar, new CdbDailyYieldRateProvider(profitability));
        var result = engine.Run(positions, start, end, rateContext);

        // Business days in (Jan 2, Jan 9] = 5 days
        var expected = Compound(10_000m, dailyRate, 5);
        Assert.Equal(expected.Balance, result.Positions[0].GrossBalance);
        Assert.Equal(expected.Yield, result.Positions[0].GrossYield);
        Assert.True(result.TotalYield > 0m);
    }

    [Fact]
    public void Engine_With110Percent_ShouldYieldMoreThan100Percent()
    {
        var start = new DateOnly(2026, 1, 2);
        var end = new DateOnly(2026, 1, 9);
        var index = RateSchedule.FromSingleRate(0.15m, start, end);
        var ipca = RateSchedule.FromSingleRate(0.05m, start, end);

        var at100 = RunWithProfitability(start, end, index, ipca, 1.00m);
        var at110 = RunWithProfitability(start, end, index, ipca, 1.10m);

        Assert.True(at110.TotalYield > at100.TotalYield);
        Assert.True(at110.TotalBalance > at100.TotalBalance);
    }

    private static DailyCalculationResult RunWithProfitability(
        DateOnly start,
        DateOnly end,
        RateSchedule index,
        RateSchedule ipca,
        decimal profitability)
    {
        var engine = new DailyCalculationEngine(Calendar, new CdbDailyYieldRateProvider(profitability));
        return engine.Run(
            [new ContributionPosition(start, 10_000m)],
            start,
            end,
            new SimulationRateContext(index, ipca));
    }

    private static (decimal Balance, decimal Yield) Compound(
        decimal balance,
        decimal dailyRate,
        int days)
    {
        var yield = 0m;
        for (var i = 0; i < days; i++)
        {
            var dailyYield = Math.Round(
                balance * dailyRate,
                MonetaryPrecision.IntermediateDecimalPlaces,
                MidpointRounding.AwayFromZero);
            balance = Math.Round(
                balance + dailyYield,
                MonetaryPrecision.IntermediateDecimalPlaces,
                MidpointRounding.AwayFromZero);
            yield = Math.Round(
                yield + dailyYield,
                MonetaryPrecision.IntermediateDecimalPlaces,
                MidpointRounding.AwayFromZero);
        }

        return (balance, yield);
    }
}
