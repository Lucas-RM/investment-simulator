using InvestmentSimulator.Domain.Calculation;
using InvestmentSimulator.Domain.Calendar;
using InvestmentSimulator.Domain.Common;
using InvestmentSimulator.Domain.Entities;
using InvestmentSimulator.Domain.Enums;
using InvestmentSimulator.Domain.Exceptions;
using InvestmentSimulator.Domain.Rates;
using InvestmentSimulator.Domain.Results;

namespace InvestmentSimulator.Application.Simulations;

/// <summary>
/// Orchestrates the full simulation pipeline: daily yield engine, B3 custody,
/// IOF, IR, inflation adjustment, and builds the final <see cref="SimulationResult"/>
/// with per-contribution details (ERS sections 19 and 20).
/// </summary>
public sealed class SimulationService
{
    private readonly FinancialCalendar _calendar;

    /// <summary>
    /// Creates the service with the given financial calendar
    /// (defaults to Brazilian national holidays when omitted).
    /// </summary>
    public SimulationService(FinancialCalendar? calendar = null)
    {
        _calendar = calendar ?? new FinancialCalendar();
    }

    /// <summary>
    /// Runs the complete simulation and returns the final summary plus
    /// per-contribution breakdown (ERS §§19–20).
    /// </summary>
    /// <param name="simulation">Validated simulation input aggregate.</param>
    /// <param name="options">
    /// Optional Tesouro ágio and B3 custody rates. When null, ágio is 0 and B3 is skipped.
    /// B3 custody is never applied to CDB simulations.
    /// </param>
    public SimulationResult Run(Simulation simulation, SimulationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(simulation);
        options ??= new SimulationOptions();

        var start = simulation.InitialContributionDate;
        var end = simulation.EndDate;

        var indexRates = BuildRateSchedule(simulation.AnnualRates, start, end);
        var ipcaRates = BuildRateSchedule(simulation.IpcaRates, start, end);

        // CDB has no B3 custody fees (ERS §14 applies to Tesouro Selic).
        var b3Rates = simulation.Type == InvestmentType.Cdb
            ? null
            : BuildOptionalB3Schedule(options.B3CustodyRates, start, end);

        var rateContext = new SimulationRateContext(indexRates, ipcaRates, b3Rates);
        var yieldProvider = CreateYieldProvider(simulation, options.AnnualAgioRate);
        var engine = new DailyCalculationEngine(_calendar, yieldProvider);

        var positions = CreateAdjustedPositions(simulation);
        var provisioner = b3Rates is null ? null : new B3CustodyProvisioner(_calendar);

        var dailyResult = engine.Run(
            positions,
            start,
            end,
            rateContext,
            afterBusinessDay: provisioner is null
                ? null
                : (businessDay, currentPositions, ctx) =>
                {
                    var custodyBalance = SumCustodyBalance(currentPositions, businessDay);
                    provisioner.ProcessBusinessDay(businessDay, custodyBalance, ctx.CurrentB3DailyRate);
                });

        var b3Collected = 0m;
        if (provisioner is not null)
        {
            provisioner.CollectOnRedemption();
            b3Collected = provisioner.TotalCollected;
        }

        ApplyTaxes(dailyResult.Positions);

        return BuildResult(simulation, dailyResult.Positions, b3Collected, ipcaRates);
    }

    private List<ContributionPosition> CreateAdjustedPositions(Simulation simulation)
    {
        var positions = new List<ContributionPosition>(1 + simulation.Contributions.Count);

        if (simulation.InitialAmount > 0m)
        {
            positions.Add(new ContributionPosition(
                _calendar.AdjustContributionDate(simulation.InitialContributionDate),
                simulation.InitialAmount));
        }

        foreach (var contribution in simulation.Contributions)
        {
            positions.Add(new ContributionPosition(
                _calendar.AdjustContributionDate(contribution.Date),
                contribution.Amount));
        }

        return positions;
    }

    private static IDailyYieldRateProvider CreateYieldProvider(
        Simulation simulation,
        decimal annualAgioRate) =>
        simulation.Type switch
        {
            InvestmentType.Cdb => new CdbDailyYieldRateProvider(simulation.ProfitabilityPercentage),
            InvestmentType.TesouroSelic => new TesouroSelicDailyYieldRateProvider(annualAgioRate),
            _ => throw new DomainValidationException(
                $"Unsupported investment type: {simulation.Type}."),
        };

    private static RateSchedule BuildRateSchedule(
        IReadOnlyList<AnnualRate> rates,
        DateOnly startDate,
        DateOnly endDate)
    {
        var years = YearGenerator.Generate(startDate, endDate);

        if (rates.Count == 1 && years.Count >= 1)
        {
            return RateSchedule.FromSingleRate(rates[0].Rate, startDate, endDate);
        }

        return RateSchedule.FromPerYear(rates, startDate, endDate);
    }

    private static RateSchedule? BuildOptionalB3Schedule(
        IReadOnlyList<AnnualRate>? b3Rates,
        DateOnly startDate,
        DateOnly endDate)
    {
        if (b3Rates is null || b3Rates.Count == 0)
        {
            return null;
        }

        return BuildRateSchedule(b3Rates, startDate, endDate);
    }

    /// <summary>
    /// Balance under custody on <paramref name="businessDay"/>: only contributions
    /// whose date is on or before that day (future aportes are not yet deposited).
    /// </summary>
    private static decimal SumCustodyBalance(
        IReadOnlyList<ContributionPosition> positions,
        DateOnly businessDay)
    {
        var total = 0m;
        foreach (var position in positions)
        {
            if (position.Date <= businessDay)
            {
                total += position.GrossBalance;
            }
        }

        return total;
    }

    private static void ApplyTaxes(IReadOnlyList<ContributionPosition> positions)
    {
        foreach (var position in positions)
        {
            var iof = IofCalculator.Calculate(position.GrossYield, position.CalendarDaysInvested);
            var yieldAfterIof = Math.Round(
                position.GrossYield - iof,
                MonetaryPrecision.IntermediateDecimalPlaces,
                MidpointRounding.AwayFromZero);
            var incomeTax = IncomeTaxCalculator.Calculate(yieldAfterIof, position.CalendarDaysInvested);
            position.SetTaxes(incomeTax, iof);
        }
    }

    private static SimulationResult BuildResult(
        Simulation simulation,
        IReadOnlyList<ContributionPosition> positions,
        decimal b3Collected,
        RateSchedule ipcaRates)
    {
        var initialAmount = simulation.InitialAmount;
        var totalAdditionalContributions = simulation.Contributions.Sum(c => c.Amount);
        var totalInvested = Math.Round(
            initialAmount + totalAdditionalContributions,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);

        var grossAmount = Math.Round(
            positions.Sum(p => p.GrossBalance),
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);

        var totalYield = Math.Round(
            positions.Sum(p => p.GrossYield),
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);

        var incomeTax = Math.Round(
            positions.Sum(p => p.IncomeTax),
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);

        var iof = Math.Round(
            positions.Sum(p => p.Iof),
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);

        // Costs come only from B3 custody (Tesouro Selic). CDB always has zero costs.
        var costs = Math.Round(
            b3Collected,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);

        var netAmount = Math.Round(
            grossAmount - costs - incomeTax - iof,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);

        if (netAmount < 0m)
        {
            netAmount = 0m;
        }

        var totalNetYield = Math.Round(
            netAmount - totalInvested,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);

        var totalGrossYield = Math.Round(
            grossAmount - totalInvested,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);

        var grossReturnPercentage = DivideReturn(totalYield, totalInvested);
        var netReturnPercentage = DivideReturn(totalNetYield, totalInvested);

        var annualIpcaFractions = ipcaRates.Rates.Select(r => r.Rate);
        var netAmountInflationAdjusted = InflationCalculator.CalculateInflationAdjustedAmount(
            netAmount,
            annualIpcaFractions);

        var details = positions.Select(p => p.ToDetail()).ToList();

        return new SimulationResult(
            simulation.InitialContributionDate,
            simulation.EndDate,
            initialAmount,
            totalAdditionalContributions,
            totalInvested,
            grossAmount,
            grossReturnPercentage,
            totalGrossYield,
            costs,
            incomeTax,
            iof,
            netAmount,
            netReturnPercentage,
            totalNetYield,
            netAmountInflationAdjusted,
            details);
    }

    private static decimal DivideReturn(decimal numerator, decimal denominator)
    {
        if (denominator == 0m)
        {
            return 0m;
        }

        return Math.Round(
            numerator / denominator,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);
    }
}
