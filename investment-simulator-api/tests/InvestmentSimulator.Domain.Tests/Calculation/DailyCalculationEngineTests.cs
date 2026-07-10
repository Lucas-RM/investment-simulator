using InvestmentSimulator.Domain.Calculation;
using InvestmentSimulator.Domain.Calendar;
using InvestmentSimulator.Domain.Common;
using InvestmentSimulator.Domain.Entities;
using InvestmentSimulator.Domain.Enums;
using InvestmentSimulator.Domain.Exceptions;
using InvestmentSimulator.Domain.Rates;

namespace InvestmentSimulator.Domain.Tests.Calculation;

public class ContributionPositionTests
{
    [Fact]
    public void Constructor_ShouldInitializeBalanceFromInitialAmount()
    {
        var position = new ContributionPosition(new DateOnly(2026, 1, 2), 10_000m);

        Assert.Equal(new DateOnly(2026, 1, 2), position.Date);
        Assert.Equal(10_000m, position.InitialAmount);
        Assert.Equal(10_000m, position.Balance);
        Assert.Equal(0m, position.Yield);
        Assert.Equal(0, position.DaysInvested);
        Assert.Equal(0m, position.IncomeTax);
        Assert.Equal(0m, position.Iof);
    }

    [Fact]
    public void Constructor_ShouldRejectNonPositiveAmount()
    {
        Assert.Throws<DomainValidationException>(() =>
            new ContributionPosition(new DateOnly(2026, 1, 2), 0m));
    }

    [Fact]
    public void IsActiveOn_ShouldBeFalse_OnContributionDate()
    {
        var date = new DateOnly(2026, 1, 2);
        var position = new ContributionPosition(date, 1_000m);

        Assert.False(position.IsActiveOn(date));
        Assert.True(position.IsActiveOn(date.AddDays(1)));
    }

    [Fact]
    public void ApplyDailyYield_ShouldUpdateBalanceAndYieldWithIntermediatePrecision()
    {
        var position = new ContributionPosition(new DateOnly(2026, 1, 2), 10_000m);
        const decimal dailyRate = 0.0005m;

        position.ApplyDailyYield(dailyRate);

        var expectedYield = Math.Round(
            10_000m * dailyRate,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);

        Assert.Equal(expectedYield, position.Yield);
        Assert.Equal(10_000m + expectedYield, position.Balance);
    }

    [Fact]
    public void UpdateDaysInvested_ShouldUseCalendarDays()
    {
        var position = new ContributionPosition(new DateOnly(2026, 1, 1), 1_000m);

        position.UpdateDaysInvested(new DateOnly(2026, 1, 31));

        Assert.Equal(30, position.DaysInvested);
    }

    [Fact]
    public void ToDetail_ShouldProjectCurrentState()
    {
        var position = new ContributionPosition(new DateOnly(2026, 1, 2), 1_000m);
        position.ApplyDailyYield(0.001m);
        position.UpdateDaysInvested(new DateOnly(2026, 1, 10));
        position.SetTaxes(10m, 1m);

        var detail = position.ToDetail();

        Assert.Equal(position.Date, detail.Date);
        Assert.Equal(position.InitialAmount, detail.Amount);
        Assert.Equal(position.Balance, detail.Balance);
        Assert.Equal(position.Yield, detail.Yield);
        Assert.Equal(position.DaysInvested, detail.DaysInvested);
        Assert.Equal(10m, detail.IncomeTax);
        Assert.Equal(1m, detail.Iof);
    }
}

public class SimulationRateContextTests
{
    [Fact]
    public void AdvanceToYear_ShouldLoadIndexIpcaAndB3Rates()
    {
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2027, 12, 31);
        var index = RateSchedule.FromPerYear(
            [new AnnualRate(2026, 0.15m), new AnnualRate(2027, 0.13m)],
            start,
            end);
        var ipca = RateSchedule.FromPerYear(
            [new AnnualRate(2026, 0.05m), new AnnualRate(2027, 0.04m)],
            start,
            end);
        var b3 = RateSchedule.FromPerYear(
            [new AnnualRate(2026, 0.0025m), new AnnualRate(2027, 0.0020m)],
            start,
            end);

        var context = new SimulationRateContext(index, ipca, b3);

        Assert.True(context.AdvanceToYear(2026));
        Assert.Equal(2026, context.CurrentYear);
        Assert.Equal(0.15m, context.CurrentIndexAnnualRate);
        Assert.Equal(RateConverter.AnnualToDaily(0.15m), context.CurrentIndexDailyRate);
        Assert.Equal(0.05m, context.CurrentIpcaAnnualRate);
        Assert.Equal(RateConverter.AnnualToDaily(0.05m), context.CurrentIpcaDailyRate);
        Assert.Equal(0.0025m, context.CurrentB3AnnualRate);
        Assert.Equal(RateConverter.AnnualToDaily(0.0025m), context.CurrentB3DailyRate);
    }

    [Fact]
    public void AdvanceToYear_ShouldSwitchRates_WhenYearChanges()
    {
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2027, 12, 31);
        var index = RateSchedule.FromPerYear(
            [new AnnualRate(2026, 0.15m), new AnnualRate(2027, 0.13m)],
            start,
            end);
        var ipca = RateSchedule.FromSingleRate(0.05m, start, end);

        var context = new SimulationRateContext(index, ipca);

        context.AdvanceToYear(2026);
        Assert.False(context.AdvanceToYear(2026));

        Assert.True(context.AdvanceToYear(2027));
        Assert.Equal(2027, context.CurrentYear);
        Assert.Equal(0.13m, context.CurrentIndexAnnualRate);
        Assert.Equal(RateConverter.AnnualToDaily(0.13m), context.CurrentIndexDailyRate);
        Assert.Equal(0m, context.CurrentB3AnnualRate);
        Assert.Equal(0m, context.CurrentB3DailyRate);
    }
}

public class DailyCalculationEngineTests
{
    private static readonly FinancialCalendar Calendar = new(isHoliday: _ => false);

    [Fact]
    public void Run_ShouldAccrueYieldIndependentlyPerContribution()
    {
        var start = new DateOnly(2026, 1, 2); // Friday
        var end = new DateOnly(2026, 1, 9);   // Friday — business days: 5,6,7,8,9
        var index = RateSchedule.FromSingleRate(0.15m, start, end);
        var ipca = RateSchedule.FromSingleRate(0.05m, start, end);
        var rateContext = new SimulationRateContext(index, ipca);

        var positions = new List<ContributionPosition>
        {
            new(start, 10_000m),
            new(new DateOnly(2026, 1, 6), 1_000m), // Tuesday
        };

        var engine = new DailyCalculationEngine(Calendar, new IndexDailyYieldRateProvider());
        var result = engine.Run(positions, start, end, rateContext);

        var dailyRate = RateConverter.AnnualToDaily(0.15m);

        // First contribution accrues on business days in (Jan 2, Jan 9] = 5,6,7,8,9 → 5 days
        var expectedFirst = Compound(10_000m, dailyRate, 5);
        Assert.Equal(expectedFirst.Balance, result.Positions[0].Balance);
        Assert.Equal(expectedFirst.Yield, result.Positions[0].Yield);
        Assert.Equal(7, result.Positions[0].DaysInvested); // Jan 2 → Jan 9

        // Second contribution accrues on business days in (Jan 6, Jan 9] = 7,8,9 → 3 days
        var expectedSecond = Compound(1_000m, dailyRate, 3);
        Assert.Equal(expectedSecond.Balance, result.Positions[1].Balance);
        Assert.Equal(expectedSecond.Yield, result.Positions[1].Yield);
        Assert.Equal(3, result.Positions[1].DaysInvested); // Jan 6 → Jan 9
    }

    [Fact]
    public void Run_ShouldSwitchRatesAutomaticallyAtYearBoundary()
    {
        // Avoid holidays with a no-holiday calendar; span two years.
        var start = new DateOnly(2026, 12, 30); // Wednesday
        var end = new DateOnly(2027, 1, 5);     // Tuesday
        var index = RateSchedule.FromPerYear(
            [new AnnualRate(2026, 0.15m), new AnnualRate(2027, 0.10m)],
            start,
            end);
        var ipca = RateSchedule.FromSingleRate(0.04m, start, end);
        var rateContext = new SimulationRateContext(index, ipca);

        var trackingProvider = new TrackingYieldRateProvider();
        var engine = new DailyCalculationEngine(Calendar, trackingProvider);
        var positions = new List<ContributionPosition> { new(start, 5_000m) };

        var result = engine.Run(positions, start, end, rateContext);

        Assert.Equal(2, result.RateSwitchCount);
        Assert.Contains(RateConverter.AnnualToDaily(0.15m), trackingProvider.RatesApplied);
        Assert.Contains(RateConverter.AnnualToDaily(0.10m), trackingProvider.RatesApplied);

        // Business days in (2026-12-30, 2027-01-05]:
        // 2026-12-31, 2027-01-01, 2027-01-02, 2027-01-05 (weekends 1/3–1/4 skipped)
        var rate2026 = RateConverter.AnnualToDaily(0.15m);
        var rate2027 = RateConverter.AnnualToDaily(0.10m);
        var expected = Compound(5_000m, rate2026, 1);
        expected = Compound(expected.Balance, rate2027, 3, expected.Yield);

        Assert.Equal(expected.Balance, result.Positions[0].Balance);
        Assert.Equal(expected.Yield, result.Positions[0].Yield);
    }

    [Fact]
    public void Run_FromSimulation_ShouldIncludeInitialAmountAndAdditionalContributions()
    {
        var start = new DateOnly(2026, 1, 2);
        var end = new DateOnly(2026, 1, 9);
        var simulation = new Simulation(
            type: InvestmentType.Cdb,
            initialAmount: 10_000m,
            initialContributionDate: start,
            endDate: end,
            contributions: [new Contribution(new DateOnly(2026, 1, 6), 500m)],
            annualRates: [new AnnualRate(2026, 0.15m)],
            ipcaRates: [new AnnualRate(2026, 0.05m)],
            profitabilityPercentage: 1.0m,
            costs: 0m);

        var index = RateSchedule.FromSingleRate(0.15m, start, end);
        var ipca = RateSchedule.FromSingleRate(0.05m, start, end);
        var engine = new DailyCalculationEngine(Calendar, new IndexDailyYieldRateProvider());

        var result = engine.Run(simulation, new SimulationRateContext(index, ipca));

        Assert.Equal(2, result.Positions.Count);
        Assert.Equal(10_000m, result.Positions[0].InitialAmount);
        Assert.Equal(500m, result.Positions[1].InitialAmount);
        Assert.True(result.TotalBalance > 10_500m);
        Assert.True(result.TotalYield > 0m);
        Assert.Equal(1, result.RateSwitchCount);
    }

    [Fact]
    public void Run_ShouldRejectEmptyPositions()
    {
        var start = new DateOnly(2026, 1, 2);
        var end = new DateOnly(2026, 1, 9);
        var index = RateSchedule.FromSingleRate(0.15m, start, end);
        var ipca = RateSchedule.FromSingleRate(0.05m, start, end);
        var engine = new DailyCalculationEngine(Calendar, new IndexDailyYieldRateProvider());

        Assert.Throws<DomainValidationException>(() =>
            engine.Run([], start, end, new SimulationRateContext(index, ipca)));
    }

    private static (decimal Balance, decimal Yield) Compound(
        decimal balance,
        decimal dailyRate,
        int days,
        decimal existingYield = 0m)
    {
        var yield = existingYield;
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

    private sealed class TrackingYieldRateProvider : IDailyYieldRateProvider
    {
        public List<decimal> RatesApplied { get; } = [];

        public decimal GetDailyYieldRate(SimulationRateContext rateContext)
        {
            RatesApplied.Add(rateContext.CurrentIndexDailyRate);
            return rateContext.CurrentIndexDailyRate;
        }
    }
}
