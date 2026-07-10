using InvestmentSimulator.Domain.Calculation;
using InvestmentSimulator.Domain.Calendar;
using InvestmentSimulator.Domain.Common;
using InvestmentSimulator.Domain.Exceptions;
using InvestmentSimulator.Domain.Rates;

namespace InvestmentSimulator.Domain.Tests.Calculation;

public class TesouroSelicCalculatorTests
{
    [Fact]
    public void CalculateDailyYieldRate_ShouldApplyErsSection13Formula()
    {
        var dailySelic = RateConverter.AnnualToDaily(0.15m);
        var dailyAgio = RateConverter.AnnualToDaily(0.001m);

        var result = TesouroSelicCalculator.CalculateDailyYieldRate(dailySelic, dailyAgio);

        var expected = Math.Round(
            (1m + dailySelic) * (1m + dailyAgio) - 1m,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateDailyYieldRate_WithZeroAgio_ShouldEqualDailySelic()
    {
        var dailySelic = RateConverter.AnnualToDaily(0.15m);

        var result = TesouroSelicCalculator.CalculateDailyYieldRate(dailySelic, 0m);

        Assert.Equal(dailySelic, result);
    }

    [Fact]
    public void CalculateDailyYieldRate_WithPositiveAgio_ShouldExceedDailySelic()
    {
        var dailySelic = RateConverter.AnnualToDaily(0.15m);
        var dailyAgio = RateConverter.AnnualToDaily(0.001m);

        var result = TesouroSelicCalculator.CalculateDailyYieldRate(dailySelic, dailyAgio);

        Assert.True(result > dailySelic);
    }

    [Fact]
    public void CalculateDailyYieldRate_WithNegativeAgio_ShouldBeBelowDailySelic()
    {
        var dailySelic = RateConverter.AnnualToDaily(0.15m);
        var dailyAgio = RateConverter.AnnualToDaily(-0.001m);

        var result = TesouroSelicCalculator.CalculateDailyYieldRate(dailySelic, dailyAgio);

        Assert.True(result < dailySelic);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-1.5)]
    public void CalculateDailyYieldRate_ShouldRejectSelicLessThanOrEqualToMinusOne(decimal dailySelic)
    {
        Assert.Throws<DomainValidationException>(() =>
            TesouroSelicCalculator.CalculateDailyYieldRate(dailySelic, 0m));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-1.5)]
    public void CalculateDailyYieldRate_ShouldRejectAgioLessThanOrEqualToMinusOne(decimal dailyAgio)
    {
        Assert.Throws<DomainValidationException>(() =>
            TesouroSelicCalculator.CalculateDailyYieldRate(0.0005m, dailyAgio));
    }

    [Fact]
    public void CalculateEffectiveAnnualRate_ShouldCompoundSelicAndAgio()
    {
        // (1.15 × 1.001) − 1 = 0.15115
        var result = TesouroSelicCalculator.CalculateEffectiveAnnualRate(0.15m, 0.001m);

        Assert.Equal(0.15115m, result);
    }

    [Fact]
    public void CalculateEffectiveAnnualRate_WithZeroAgio_ShouldEqualAnnualSelic()
    {
        var result = TesouroSelicCalculator.CalculateEffectiveAnnualRate(0.15m, 0m);

        Assert.Equal(0.15m, result);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-2)]
    public void CalculateEffectiveAnnualRate_ShouldRejectInvalidAgio(decimal annualAgio)
    {
        Assert.Throws<DomainValidationException>(() =>
            TesouroSelicCalculator.CalculateEffectiveAnnualRate(0.15m, annualAgio));
    }
}

public class TesouroSelicDailyYieldRateProviderTests
{
    private static readonly FinancialCalendar Calendar = new(isHoliday: _ => false);

    [Fact]
    public void Constructor_ShouldRejectAgioLessThanOrEqualToMinusOne()
    {
        Assert.Throws<DomainValidationException>(() => new TesouroSelicDailyYieldRateProvider(-1m));
        Assert.Throws<DomainValidationException>(() => new TesouroSelicDailyYieldRateProvider(-1.5m));
    }

    [Fact]
    public void Constructor_ShouldConvertAnnualAgioToDaily()
    {
        const decimal annualAgio = 0.001m;
        var provider = new TesouroSelicDailyYieldRateProvider(annualAgio);

        Assert.Equal(annualAgio, provider.AnnualAgioRate);
        Assert.Equal(RateConverter.AnnualToDaily(annualAgio), provider.DailyAgioRate);
    }

    [Fact]
    public void GetDailyYieldRate_ShouldApplySelicTimesAgioCompound()
    {
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 12, 31);
        var index = RateSchedule.FromSingleRate(0.15m, start, end);
        var ipca = RateSchedule.FromSingleRate(0.05m, start, end);
        var context = new SimulationRateContext(index, ipca);
        context.AdvanceToYear(2026);

        const decimal annualAgio = 0.001m;
        var provider = new TesouroSelicDailyYieldRateProvider(annualAgio);
        var result = provider.GetDailyYieldRate(context);

        Assert.Equal(
            TesouroSelicCalculator.CalculateDailyYieldRate(
                context.CurrentIndexDailyRate,
                RateConverter.AnnualToDaily(annualAgio)),
            result);
    }

    [Fact]
    public void GetDailyYieldRate_ShouldRejectNullContext()
    {
        var provider = new TesouroSelicDailyYieldRateProvider(0m);

        Assert.Throws<ArgumentNullException>(() => provider.GetDailyYieldRate(null!));
    }

    [Fact]
    public void Engine_WithTesouroProvider_ShouldAccrueAtCompoundedSelicAndAgio()
    {
        var start = new DateOnly(2026, 1, 2); // Friday
        var end = new DateOnly(2026, 1, 9);   // Friday — business days: 5,6,7,8,9
        var index = RateSchedule.FromSingleRate(0.15m, start, end);
        var ipca = RateSchedule.FromSingleRate(0.05m, start, end);
        var rateContext = new SimulationRateContext(index, ipca);

        const decimal annualAgio = 0.001m;
        var dailyRate = TesouroSelicCalculator.CalculateDailyYieldRate(
            RateConverter.AnnualToDaily(0.15m),
            RateConverter.AnnualToDaily(annualAgio));

        var positions = new List<ContributionPosition> { new(start, 10_000m) };
        var engine = new DailyCalculationEngine(
            Calendar,
            new TesouroSelicDailyYieldRateProvider(annualAgio));
        var result = engine.Run(positions, start, end, rateContext);

        // Business days in (Jan 2, Jan 9] = 5 days
        var expected = Compound(10_000m, dailyRate, 5);
        Assert.Equal(expected.Balance, result.Positions[0].GrossBalance);
        Assert.Equal(expected.Yield, result.Positions[0].GrossYield);
        Assert.True(result.TotalYield > 0m);
    }

    [Fact]
    public void Engine_WithPositiveAgio_ShouldYieldMoreThanZeroAgio()
    {
        var start = new DateOnly(2026, 1, 2);
        var end = new DateOnly(2026, 1, 9);
        var index = RateSchedule.FromSingleRate(0.15m, start, end);
        var ipca = RateSchedule.FromSingleRate(0.05m, start, end);

        var withZeroAgio = RunWithAgio(start, end, index, ipca, 0m);
        var withPositiveAgio = RunWithAgio(start, end, index, ipca, 0.001m);

        Assert.True(withPositiveAgio.TotalYield > withZeroAgio.TotalYield);
        Assert.True(withPositiveAgio.TotalBalance > withZeroAgio.TotalBalance);
    }

    [Fact]
    public void Engine_WithNegativeAgio_ShouldYieldLessThanZeroAgio()
    {
        var start = new DateOnly(2026, 1, 2);
        var end = new DateOnly(2026, 1, 9);
        var index = RateSchedule.FromSingleRate(0.15m, start, end);
        var ipca = RateSchedule.FromSingleRate(0.05m, start, end);

        var withZeroAgio = RunWithAgio(start, end, index, ipca, 0m);
        var withNegativeAgio = RunWithAgio(start, end, index, ipca, -0.001m);

        Assert.True(withNegativeAgio.TotalYield < withZeroAgio.TotalYield);
        Assert.True(withNegativeAgio.TotalBalance < withZeroAgio.TotalBalance);
    }

    private static DailyCalculationResult RunWithAgio(
        DateOnly start,
        DateOnly end,
        RateSchedule index,
        RateSchedule ipca,
        decimal annualAgio)
    {
        var engine = new DailyCalculationEngine(
            Calendar,
            new TesouroSelicDailyYieldRateProvider(annualAgio));
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
