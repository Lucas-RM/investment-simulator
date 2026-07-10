using InvestmentSimulator.Application.Simulations;
using InvestmentSimulator.Domain.Calendar;
using InvestmentSimulator.Domain.Common;
using InvestmentSimulator.Domain.Entities;
using InvestmentSimulator.Domain.Enums;

namespace InvestmentSimulator.Application.Tests.Simulations;

public class SimulationComparisonServiceTests
{
    /// <summary>Calendar without holidays so day counts stay deterministic.</summary>
    private static readonly FinancialCalendar Calendar = new(isHoliday: _ => false);

    [Fact]
    public void Compare_CdbVsTesouroSelic_ShouldExposeErsSection24Metrics()
    {
        var start = new DateOnly(2026, 1, 2);
        var end = new DateOnly(2026, 7, 2);

        var cdb = CreateCdbSimulation(start, end, initialAmount: 10_000m);
        var tesouro = CreateTesouroSimulation(start, end, initialAmount: 10_000m);

        var comparison = new SimulationComparisonService(Calendar).Compare(
            cdb,
            tesouro,
            leftOptions: null,
            rightOptions: new SimulationOptions
            {
                AnnualAgioRate = 0.001m,
                B3Rates = [new AnnualRate(2026, 0.0025m)],
            });

        Assert.Equal(InvestmentType.Cdb, comparison.Left.Type);
        Assert.Equal(InvestmentType.TesouroSelic, comparison.Right.Type);

        Assert.True(comparison.Left.NetAmount > 0m);
        Assert.True(comparison.Right.NetAmount > 0m);
        Assert.True(comparison.Left.IncomeTax >= 0m);
        Assert.True(comparison.Right.IncomeTax >= 0m);
        Assert.Equal(0m, comparison.Left.Costs);
        Assert.True(comparison.Right.Costs > 0m); // B3 custody on Tesouro side
        Assert.True(comparison.Left.InflationAdjustedAmount > 0m);
        Assert.True(comparison.Right.InflationAdjustedAmount > 0m);
        Assert.Equal(
            Round(comparison.Right.NetAmount - comparison.Left.NetAmount),
            comparison.NetAmountDifference);
    }

    [Fact]
    public void Compare_ShouldComputeDifferencesAsRightMinusLeft()
    {
        var start = new DateOnly(2026, 1, 2);
        var end = new DateOnly(2026, 3, 2);

        var left = CreateCdbSimulation(start, end, initialAmount: 10_000m);
        var right = CreateTesouroSimulation(start, end, initialAmount: 10_000m);

        var simulationService = new SimulationService(Calendar);
        var leftResult = simulationService.Run(left);
        var rightResult = simulationService.Run(
            right,
            new SimulationOptions { AnnualAgioRate = 0.001m });

        var comparison = new SimulationComparisonService(simulationService).Compare(
            left,
            right,
            rightOptions: new SimulationOptions { AnnualAgioRate = 0.001m });

        Assert.Equal(leftResult.NetAmount, comparison.Left.NetAmount);
        Assert.Equal(leftResult.IncomeTax, comparison.Left.IncomeTax);
        Assert.Equal(leftResult.Costs, comparison.Left.Costs);
        Assert.Equal(leftResult.NetProfit, comparison.Left.NetProfit);
        Assert.Equal(leftResult.NetReturn, comparison.Left.NetReturn);
        Assert.Equal(leftResult.InflationAdjustedAmount, comparison.Left.InflationAdjustedAmount);

        Assert.Equal(rightResult.NetAmount, comparison.Right.NetAmount);
        Assert.Equal(rightResult.IncomeTax, comparison.Right.IncomeTax);
        Assert.Equal(rightResult.Costs, comparison.Right.Costs);
        Assert.Equal(rightResult.NetProfit, comparison.Right.NetProfit);
        Assert.Equal(rightResult.NetReturn, comparison.Right.NetReturn);
        Assert.Equal(rightResult.InflationAdjustedAmount, comparison.Right.InflationAdjustedAmount);

        Assert.Equal(
            Round(rightResult.NetAmount - leftResult.NetAmount),
            comparison.NetAmountDifference);
        Assert.Equal(
            Round(rightResult.IncomeTax - leftResult.IncomeTax),
            comparison.IncomeTaxDifference);
        Assert.Equal(
            Round(rightResult.Costs - leftResult.Costs),
            comparison.CostsDifference);
        Assert.Equal(
            Round(rightResult.NetProfit - leftResult.NetProfit),
            comparison.NetProfitDifference);
        Assert.Equal(
            Round(rightResult.NetReturn - leftResult.NetReturn),
            comparison.NetReturnDifference);
        Assert.Equal(
            Round(rightResult.InflationAdjustedAmount - leftResult.InflationAdjustedAmount),
            comparison.InflationAdjustedAmountDifference);
    }

    [Fact]
    public void Compare_SameInputs_ShouldProduceZeroDifferences()
    {
        var start = new DateOnly(2026, 1, 2);
        var end = new DateOnly(2026, 2, 2);
        var simulation = CreateCdbSimulation(start, end, initialAmount: 5_000m);

        var comparison = new SimulationComparisonService(Calendar).Compare(simulation, simulation);

        Assert.Equal(0m, comparison.NetAmountDifference);
        Assert.Equal(0m, comparison.IncomeTaxDifference);
        Assert.Equal(0m, comparison.CostsDifference);
        Assert.Equal(0m, comparison.NetProfitDifference);
        Assert.Equal(0m, comparison.NetReturnDifference);
        Assert.Equal(0m, comparison.InflationAdjustedAmountDifference);
        Assert.Equal(comparison.Left.NetAmount, comparison.Right.NetAmount);
    }

    [Fact]
    public void Compare_ShouldRejectNullSimulations()
    {
        var service = new SimulationComparisonService(Calendar);
        var valid = CreateCdbSimulation(
            new DateOnly(2026, 1, 2),
            new DateOnly(2026, 1, 9),
            initialAmount: 1_000m);

        Assert.Throws<ArgumentNullException>(() => service.Compare(null!, valid));
        Assert.Throws<ArgumentNullException>(() => service.Compare(valid, null!));
    }

    private static decimal Round(decimal value) =>
        Math.Round(
            value,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);

    private static Simulation CreateCdbSimulation(
        DateOnly start,
        DateOnly end,
        decimal initialAmount) =>
        new(
            type: InvestmentType.Cdb,
            initialAmount: initialAmount,
            initialContributionDate: start,
            endDate: end,
            contributions: [],
            annualRates: [new AnnualRate(start.Year, 0.15m)],
            ipcaRates: [new AnnualRate(start.Year, 0.05m)],
            profitabilityPercentage: 1.10m,
            costs: 0m);

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
            ipcaRates: [new AnnualRate(start.Year, 0.05m)],
            profitabilityPercentage: 1.0m,
            costs: 0m);
}
