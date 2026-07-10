using System.Diagnostics;
using InvestmentSimulator.Domain.Calculation;
using InvestmentSimulator.Domain.Calendar;
using InvestmentSimulator.Domain.Common;
using InvestmentSimulator.Domain.Entities;
using InvestmentSimulator.Domain.Enums;
using InvestmentSimulator.Domain.Rates;

namespace InvestmentSimulator.Domain.Tests.Calculation;

/// <summary>
/// Cross-cutting automated tests that validate financial calculator composition
/// (ERS section 30): IR, IOF, B3 custody, inflation and the daily calculation engine.
/// </summary>
public class FinancialCalculatorsValidationTests
{
    private static readonly FinancialCalendar Calendar = new(isHoliday: _ => false);

    [Fact]
    public void IofThenIncomeTax_ShouldTaxYieldAfterIofDeduction()
    {
        // Holding < 30 days → IOF applies; IR uses yield net of IOF (orchestrator rule).
        const decimal yield = 1_000m;
        const int daysInvested = 15; // IOF 50%, IR 22.5%

        var iof = IofCalculator.Calculate(yield, daysInvested);
        var yieldAfterIof = Math.Round(
            yield - iof,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);
        var incomeTax = IncomeTaxCalculator.Calculate(yieldAfterIof, daysInvested);

        Assert.Equal(500m, iof);
        Assert.Equal(500m, yieldAfterIof);
        Assert.Equal(112.5m, incomeTax);

        // IR on gross yield would be higher — composition must use net-of-IOF base.
        Assert.True(incomeTax < IncomeTaxCalculator.Calculate(yield, daysInvested));
    }

    [Fact]
    public void IofThenIncomeTax_WhenExemptFromIof_ShouldTaxFullYield()
    {
        const decimal yield = 2_000m;
        const int daysInvested = 400; // IOF exempt; IR 17.5%

        var iof = IofCalculator.Calculate(yield, daysInvested);
        var incomeTax = IncomeTaxCalculator.Calculate(yield - iof, daysInvested);

        Assert.Equal(0m, iof);
        Assert.Equal(350m, incomeTax);
    }

    [Theory]
    [InlineData(90, 0.225)]
    [InlineData(200, 0.20)]
    [InlineData(500, 0.175)]
    [InlineData(800, 0.15)]
    public void IncomeTax_PerContribution_ShouldUseOwnHoldingPeriod(
        int daysInvested,
        decimal expectedRate)
    {
        const decimal yield = 1_000m;

        Assert.Equal(expectedRate, IncomeTaxCalculator.GetRate(daysInvested));
        Assert.Equal(
            Math.Round(
                yield * expectedRate,
                MonetaryPrecision.IntermediateDecimalPlaces,
                MidpointRounding.AwayFromZero),
            IncomeTaxCalculator.Calculate(yield, daysInvested));
    }

    [Fact]
    public void EngineWithB3CustodyHook_ShouldProvisionAndCollectOnRedemption()
    {
        var start = new DateOnly(2026, 1, 2);
        var end = new DateOnly(2026, 1, 9);
        var index = RateSchedule.FromSingleRate(0.15m, start, end);
        var ipca = RateSchedule.FromSingleRate(0.04m, start, end);
        var b3 = RateSchedule.FromSingleRate(0.002m, start, end);
        var rateContext = new SimulationRateContext(index, ipca, b3);
        var provisioner = new B3CustodyProvisioner(Calendar);
        var positions = new List<ContributionPosition> { new(start, 20_000m) };

        var engine = new DailyCalculationEngine(Calendar, new IndexDailyYieldRateProvider());
        engine.Run(
            positions,
            start,
            end,
            rateContext,
            afterBusinessDay: (day, currentPositions, ctx) =>
            {
                var balance = currentPositions.Sum(p => p.Balance);
                provisioner.ProcessBusinessDay(day, balance, ctx.CurrentB3DailyRate);
            });

        var collectedAtRedemption = provisioner.CollectOnRedemption();

        Assert.True(positions[0].Yield > 0m);
        Assert.True(collectedAtRedemption > 0m);
        Assert.Equal(collectedAtRedemption, provisioner.TotalCollected);
        Assert.Equal(0m, provisioner.ProvisionedAmount);

        // Excess over R$10,000 is the only taxable base.
        Assert.Equal(10_000m, B3CustodyCalculator.CalculateTaxableBase(20_000m));
    }

    [Fact]
    public void EngineWithB3CustodyHook_WhenBalanceExempt_ShouldNotChargeCustody()
    {
        var start = new DateOnly(2026, 1, 2);
        var end = new DateOnly(2026, 1, 9);
        var index = RateSchedule.FromSingleRate(0.15m, start, end);
        var ipca = RateSchedule.FromSingleRate(0.04m, start, end);
        var b3 = RateSchedule.FromSingleRate(0.002m, start, end);
        var rateContext = new SimulationRateContext(index, ipca, b3);
        var provisioner = new B3CustodyProvisioner(Calendar);
        var positions = new List<ContributionPosition> { new(start, 8_000m) };

        var engine = new DailyCalculationEngine(Calendar, new IndexDailyYieldRateProvider());
        engine.Run(
            positions,
            start,
            end,
            rateContext,
            afterBusinessDay: (day, currentPositions, ctx) =>
            {
                var balance = currentPositions.Sum(p => p.Balance);
                provisioner.ProcessBusinessDay(day, balance, ctx.CurrentB3DailyRate);
            });

        Assert.Equal(0m, provisioner.CollectOnRedemption());
        Assert.Equal(0m, provisioner.TotalCollected);
    }

    [Fact]
    public void NetAmountAfterTaxesAndCosts_ShouldFeedInflationAdjustment()
    {
        const decimal grossAmount = 12_000m;
        const decimal totalInvested = 10_000m;
        const decimal costs = 25m;
        const decimal yield = 2_000m;
        const int daysInvested = 400;

        var iof = IofCalculator.Calculate(yield, daysInvested);
        var incomeTax = IncomeTaxCalculator.Calculate(yield - iof, daysInvested);
        var netAmount = Math.Round(
            grossAmount - costs - incomeTax - iof,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);

        decimal[] ipcaRates = [0.05m, 0.04m, 0.045m];
        var realAmount = InflationCalculator.CalculateInflationAdjustedAmount(netAmount, ipcaRates);
        var accumulated = InflationCalculator.CalculateAccumulatedInflation(ipcaRates);

        Assert.Equal(0m, iof);
        Assert.Equal(350m, incomeTax);
        Assert.Equal(11_625m, netAmount);
        Assert.Equal(0.14114m, accumulated);
        Assert.Equal(
            InflationCalculator.AdjustForPurchasingPower(netAmount, accumulated),
            realAmount);
        Assert.True(realAmount < netAmount);
        Assert.True(netAmount > totalInvested);
    }

    [Fact]
    public void ContributionPosition_SetTaxes_ShouldPersistIofAndIncomeTax()
    {
        var position = new ContributionPosition(new DateOnly(2026, 1, 2), 5_000m);
        position.ApplyDailyYield(0.001m);
        position.UpdateDaysInvested(new DateOnly(2026, 1, 20));

        var iof = IofCalculator.Calculate(position.Yield, position.DaysInvested);
        var incomeTax = IncomeTaxCalculator.Calculate(position.Yield - iof, position.DaysInvested);
        position.SetTaxes(incomeTax, iof);

        var detail = position.ToDetail();
        Assert.Equal(iof, detail.Iof);
        Assert.Equal(incomeTax, detail.IncomeTax);
        Assert.Equal(position.DaysInvested, detail.DaysInvested);
    }

    [Fact]
    public void LongSimulation_FiftyYears_ShouldCompleteWithinFewSeconds()
    {
        var start = new DateOnly(2026, 1, 2);
        var end = start.AddYears(50);
        var years = YearGenerator.Generate(start, end);
        var annualRates = years.Select(y => new AnnualRate(y, 0.12m)).ToList();
        var ipcaRates = years.Select(y => new AnnualRate(y, 0.04m)).ToList();

        var simulation = new Simulation(
            type: InvestmentType.Cdb,
            initialAmount: 10_000m,
            initialContributionDate: start,
            endDate: end,
            contributions: [],
            annualRates: annualRates,
            ipcaRates: ipcaRates,
            profitabilityPercentage: 1.0m,
            costs: 0m);

        var index = RateSchedule.FromPerYear(annualRates, start, end);
        var ipca = RateSchedule.FromPerYear(ipcaRates, start, end);
        var engine = new DailyCalculationEngine(Calendar, new IndexDailyYieldRateProvider());

        var stopwatch = Stopwatch.StartNew();
        var result = engine.Run(simulation, new SimulationRateContext(index, ipca));
        stopwatch.Stop();

        Assert.True(result.TotalBalance > 10_000m);
        Assert.True(result.TotalYield > 0m);
        Assert.Equal(years.Count, result.RateSwitchCount);
        // ERS §30: long simulations (up to 50 years) in a few seconds.
        Assert.True(
            stopwatch.Elapsed < TimeSpan.FromSeconds(5),
            $"50-year simulation took {stopwatch.Elapsed.TotalSeconds:F2}s (limit 5s).");
    }

    [Fact]
    public void DailyEngine_MultipleContributions_ShouldKeepIndependentTaxBases()
    {
        var start = new DateOnly(2026, 1, 2);
        var end = new DateOnly(2026, 2, 2); // 31 calendar days from start → first aporte IOF-exempt
        var index = RateSchedule.FromSingleRate(0.15m, start, end);
        var ipca = RateSchedule.FromSingleRate(0.05m, start, end);
        var engine = new DailyCalculationEngine(Calendar, new IndexDailyYieldRateProvider());

        var positions = new List<ContributionPosition>
        {
            new(start, 10_000m),
            new(new DateOnly(2026, 1, 20), 2_000m),
        };

        var result = engine.Run(positions, start, end, new SimulationRateContext(index, ipca));

        var first = result.Positions[0];
        var second = result.Positions[1];

        Assert.Equal(31, first.DaysInvested);
        Assert.Equal(13, second.DaysInvested);
        Assert.True(first.Yield > second.Yield);

        var firstIof = IofCalculator.Calculate(first.Yield, first.DaysInvested);
        var secondIof = IofCalculator.Calculate(second.Yield, second.DaysInvested);

        // First contribution held ≥ 30 calendar days → IOF exempt; second still taxable.
        Assert.Equal(0m, firstIof);
        Assert.True(second.DaysInvested < IofCalculator.ExemptionDays);
        Assert.True(secondIof > 0m);
    }
}
