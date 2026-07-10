using InvestmentSimulator.Application.Simulations;
using InvestmentSimulator.Domain.Calculation;
using InvestmentSimulator.Domain.Calendar;
using InvestmentSimulator.Domain.Common;
using InvestmentSimulator.Domain.Entities;
using InvestmentSimulator.Domain.Enums;
using InvestmentSimulator.Domain.Rates;

namespace InvestmentSimulator.Application.Tests.Simulations;

public class SimulationServiceTests
{
    /// <summary>Calendar without holidays so day counts stay deterministic.</summary>
    private static readonly FinancialCalendar Calendar = new(isHoliday: _ => false);

    [Fact]
    public void Run_Cdb_ShouldProduceFinalSummaryAndContributionDetails()
    {
        var start = new DateOnly(2026, 1, 2);
        var end = new DateOnly(2026, 1, 9);
        var simulation = new Simulation(
            type: InvestmentType.Cdb,
            initialAmount: 10_000m,
            initialContributionDate: start,
            endDate: end,
            contributions: [new Contribution(new DateOnly(2026, 1, 6), 1_000m)],
            annualRates: [new AnnualRate(2026, 0.15m)],
            ipcaRates: [new AnnualRate(2026, 0.05m)],
            profitabilityPercentage: 1.10m);

        var service = new SimulationService(Calendar);
        var result = service.Run(simulation);

        Assert.Equal(10_000m, result.InitialAmount);
        Assert.Equal(1_000m, result.TotalAdditionalContributions);
        Assert.Equal(11_000m, result.TotalInvested);
        Assert.Equal(2, result.ContributionDetails.Count);

        Assert.True(result.GrossAmount > result.TotalInvested);
        Assert.True(result.GrossReturnPercentage > 0m);
        Assert.Equal(0m, result.Costs);

        var expectedGrossReturn = Math.Round(
            (result.GrossAmount - result.TotalInvested) / result.TotalInvested,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);
        Assert.Equal(expectedGrossReturn, result.GrossReturnPercentage);

        Assert.Equal(
            Math.Round(
                result.GrossAmount - result.Costs - result.IncomeTax - result.Iof,
                MonetaryPrecision.IntermediateDecimalPlaces,
                MidpointRounding.AwayFromZero),
            result.NetAmount);

        Assert.Equal(
            Math.Round(
                result.NetAmount - result.TotalInvested,
                MonetaryPrecision.IntermediateDecimalPlaces,
                MidpointRounding.AwayFromZero),
            result.TotalNetYield);

        var expectedNetReturn = Math.Round(
            result.TotalNetYield / result.TotalInvested,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);
        Assert.Equal(expectedNetReturn, result.NetReturnPercentage);

        var expectedInflationAdjusted = InflationCalculator.CalculateInflationAdjustedAmount(
            result.NetAmount,
            [0.05m]);
        Assert.Equal(expectedInflationAdjusted, result.NetAmountInflationAdjusted);

        Assert.Equal(start, result.ContributionDetails[0].Date);
        Assert.Equal(10_000m, result.ContributionDetails[0].Amount);
        Assert.Equal(new DateOnly(2026, 1, 6), result.ContributionDetails[1].Date);
        Assert.Equal(1_000m, result.ContributionDetails[1].Amount);

        Assert.All(result.ContributionDetails, d =>
        {
            Assert.True(d.GrossBalance >= d.Amount);
            Assert.True(d.CalendarDaysInvested >= 0);
            Assert.True(d.IncomeTax >= 0m);
            Assert.True(d.Iof >= 0m);
        });
    }

    [Fact]
    public void Run_ShouldApplyIofThenIrOnYieldAfterIof_ForShortHolding()
    {
        // 10 calendar days → IOF applies; IR on yield after IOF.
        var start = new DateOnly(2026, 1, 2);
        var end = new DateOnly(2026, 1, 12);
        var simulation = new Simulation(
            type: InvestmentType.Cdb,
            initialAmount: 10_000m,
            initialContributionDate: start,
            endDate: end,
            contributions: [],
            annualRates: [new AnnualRate(2026, 0.15m)],
            ipcaRates: [new AnnualRate(2026, 0m)],
            profitabilityPercentage: 1.0m);

        var result = new SimulationService(Calendar).Run(simulation);
        var detail = Assert.Single(result.ContributionDetails);

        Assert.Equal(10, detail.CalendarDaysInvested);
        Assert.True(detail.GrossYield > 0m);

        var expectedIof = IofCalculator.Calculate(detail.GrossYield, detail.CalendarDaysInvested);
        var yieldAfterIof = Math.Round(
            detail.GrossYield - expectedIof,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);
        var expectedIr = IncomeTaxCalculator.Calculate(yieldAfterIof, detail.CalendarDaysInvested);

        Assert.Equal(expectedIof, detail.Iof);
        Assert.Equal(expectedIr, detail.IncomeTax);
        Assert.Equal(expectedIof, result.Iof);
        Assert.Equal(expectedIr, result.IncomeTax);
        Assert.True(result.Iof > 0m);
    }

    [Fact]
    public void Run_ShouldExemptIof_WhenHoldingAtLeast30Days()
    {
        var start = new DateOnly(2026, 1, 2);
        var end = new DateOnly(2026, 2, 2); // 31 calendar days
        var simulation = new Simulation(
            type: InvestmentType.Cdb,
            initialAmount: 5_000m,
            initialContributionDate: start,
            endDate: end,
            contributions: [],
            annualRates: [new AnnualRate(2026, 0.15m)],
            ipcaRates: [new AnnualRate(2026, 0.04m)],
            profitabilityPercentage: 1.0m);

        var result = new SimulationService(Calendar).Run(simulation);
        var detail = Assert.Single(result.ContributionDetails);

        Assert.True(detail.CalendarDaysInvested >= 30);
        Assert.Equal(0m, detail.Iof);
        Assert.Equal(0m, result.Iof);
        Assert.True(detail.IncomeTax > 0m);
    }

    [Fact]
    public void Run_TesouroSelic_WithAgio_ShouldYieldMoreThanZeroAgio()
    {
        var start = new DateOnly(2026, 1, 2);
        var end = new DateOnly(2026, 3, 2);
        var simulation = CreateTesouroSimulation(start, end, initialAmount: 10_000m);

        var service = new SimulationService(Calendar);
        var withZeroAgio = service.Run(simulation, new SimulationOptions { AnnualAgioRate = 0m });
        var withPositiveAgio = service.Run(simulation, new SimulationOptions { AnnualAgioRate = 0.001m });

        Assert.True(withPositiveAgio.GrossAmount > withZeroAgio.GrossAmount);
        Assert.True(withPositiveAgio.NetAmount > withZeroAgio.NetAmount);
    }

    [Fact]
    public void Run_WithB3CustodyRates_ShouldIncludeCustodyInCosts_ForTesouroOnly()
    {
        var start = new DateOnly(2026, 1, 2);
        var end = new DateOnly(2026, 7, 2);
        var tesouro = CreateTesouroSimulation(start, end, initialAmount: 50_000m);
        var cdb = new Simulation(
            type: InvestmentType.Cdb,
            initialAmount: 50_000m,
            initialContributionDate: start,
            endDate: end,
            contributions: [],
            annualRates: [new AnnualRate(2026, 0.15m)],
            ipcaRates: [new AnnualRate(2026, 0.05m)],
            profitabilityPercentage: 1.0m);

        var b3Options = new SimulationOptions
        {
            B3CustodyRates = [new AnnualRate(2026, 0.0025m)],
        };

        var withoutB3 = new SimulationService(Calendar).Run(tesouro);
        var withB3 = new SimulationService(Calendar).Run(tesouro, b3Options);
        var cdbWithB3Options = new SimulationService(Calendar).Run(cdb, b3Options);

        Assert.Equal(0m, withoutB3.Costs);
        Assert.True(withB3.Costs > 0m);
        Assert.True(withB3.NetAmount < withoutB3.NetAmount);
        // CDB ignores B3 custody rates — costs always remain zero.
        Assert.Equal(0m, cdbWithB3Options.Costs);
    }

    [Fact]
    public void Run_ShouldExpandSingleAnnualRateAcrossPeriodYears()
    {
        var start = new DateOnly(2026, 12, 30);
        var end = new DateOnly(2027, 1, 5);
        var simulation = new Simulation(
            type: InvestmentType.Cdb,
            initialAmount: 5_000m,
            initialContributionDate: start,
            endDate: end,
            contributions: [],
            annualRates: [new AnnualRate(2026, 0.15m)],
            ipcaRates: [new AnnualRate(2026, 0.04m)],
            profitabilityPercentage: 1.0m);

        var result = new SimulationService(Calendar).Run(simulation);

        Assert.True(result.GrossAmount > result.TotalInvested);
        Assert.Single(result.ContributionDetails);
    }

    [Fact]
    public void Run_ShouldRejectNullSimulation()
    {
        var service = new SimulationService(Calendar);

        Assert.Throws<ArgumentNullException>(() => service.Run(null!));
    }

    [Fact]
    public void Run_PerContributionDetails_ShouldMatchErsSection20Columns()
    {
        var start = new DateOnly(2026, 1, 2);
        var end = new DateOnly(2026, 6, 30);
        var simulation = new Simulation(
            type: InvestmentType.Cdb,
            initialAmount: 10_000m,
            initialContributionDate: start,
            endDate: end,
            contributions:
            [
                new Contribution(new DateOnly(2026, 2, 2), 900m),
                new Contribution(new DateOnly(2026, 3, 2), 1_200m),
            ],
            annualRates: [new AnnualRate(2026, 0.15m)],
            ipcaRates: [new AnnualRate(2026, 0.05m)],
            profitabilityPercentage: 1.10m);

        var result = new SimulationService(Calendar).Run(simulation);

        Assert.Equal(3, result.ContributionDetails.Count);
        Assert.Equal(result.ContributionDetails.Sum(d => d.IncomeTax), result.IncomeTax);
        Assert.Equal(result.ContributionDetails.Sum(d => d.Iof), result.Iof);
        Assert.Equal(
            Math.Round(
                result.ContributionDetails.Sum(d => d.GrossBalance),
                MonetaryPrecision.IntermediateDecimalPlaces,
                MidpointRounding.AwayFromZero),
            result.GrossAmount);
    }

    private static Simulation CreateTesouroSimulation(
        DateOnly start,
        DateOnly end,
        decimal initialAmount) =>
        new(
            type: InvestmentType.TesouroSelic,
            initialAmount: initialAmount,
            initialContributionDate: start,
            endDate: end,
            contributions: [],
            annualRates: [new AnnualRate(start.Year, 0.1475m)],
            ipcaRates: [new AnnualRate(start.Year, 0.045m)],
            profitabilityPercentage: 1.0m);
}
