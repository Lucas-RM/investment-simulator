using InvestmentSimulator.Domain.Calendar;
using InvestmentSimulator.Domain.Entities;
using InvestmentSimulator.Domain.Exceptions;

namespace InvestmentSimulator.Domain.Calculation;

/// <summary>
/// Base daily calculation engine (ERS sections 9–11).
/// For each business day, updates every active contribution's balance, yield and
/// days invested, switching annual rates automatically at year boundaries.
/// Product-specific yield formulas (CDB / Tesouro Selic) are supplied via
/// <see cref="IDailyYieldRateProvider"/>.
/// </summary>
public sealed class DailyCalculationEngine
{
    private readonly FinancialCalendar _calendar;
    private readonly IDailyYieldRateProvider _yieldRateProvider;

    public DailyCalculationEngine(
        FinancialCalendar calendar,
        IDailyYieldRateProvider yieldRateProvider)
    {
        ArgumentNullException.ThrowIfNull(calendar);
        ArgumentNullException.ThrowIfNull(yieldRateProvider);

        _calendar = calendar;
        _yieldRateProvider = yieldRateProvider;
    }

    /// <summary>
    /// Runs the daily loop from the simulation's initial contribution date to the
    /// redemption date, treating the initial amount and each additional contribution
    /// as independent positions (ERS section 9).
    /// </summary>
    /// <param name="afterBusinessDay">
    /// Optional hook invoked after yields are applied for each business day
    /// (e.g. B3 custody provisioning by the orchestrator).
    /// </param>
    public DailyCalculationResult Run(
        Simulation simulation,
        SimulationRateContext rateContext,
        Action<DateOnly, IReadOnlyList<ContributionPosition>, SimulationRateContext>? afterBusinessDay = null)
    {
        ArgumentNullException.ThrowIfNull(simulation);
        ArgumentNullException.ThrowIfNull(rateContext);

        var positions = CreatePositions(simulation);
        return Run(
            positions,
            simulation.InitialContributionDate,
            simulation.EndDate,
            rateContext,
            afterBusinessDay);
    }

    /// <summary>
    /// Runs the daily loop over the given positions between
    /// <paramref name="startDate"/> and <paramref name="endDate"/> (ERS section 10).
    /// </summary>
    /// <param name="afterBusinessDay">
    /// Optional hook invoked after yields are applied for each business day
    /// (e.g. B3 custody provisioning by the orchestrator).
    /// </param>
    public DailyCalculationResult Run(
        IReadOnlyList<ContributionPosition> positions,
        DateOnly startDate,
        DateOnly endDate,
        SimulationRateContext rateContext,
        Action<DateOnly, IReadOnlyList<ContributionPosition>, SimulationRateContext>? afterBusinessDay = null)
    {
        ArgumentNullException.ThrowIfNull(positions);
        ArgumentNullException.ThrowIfNull(rateContext);

        if (positions.Count == 0)
        {
            throw new DomainValidationException("At least one contribution position is required.");
        }

        if (startDate == default || endDate == default)
        {
            throw new DomainValidationException("Start and end dates are required and must be valid dates.");
        }

        if (endDate < startDate)
        {
            throw new DomainValidationException("End date cannot be earlier than start date.");
        }

        foreach (var position in positions)
        {
            if (position is null)
            {
                throw new DomainValidationException("Contribution positions cannot contain null entries.");
            }
        }

        var rateSwitches = 0;

        foreach (var businessDay in _calendar.EnumerateBusinessDays(startDate, endDate))
        {
            if (rateContext.AdvanceToYear(businessDay.Year))
            {
                rateSwitches++;
            }

            var dailyYieldRate = _yieldRateProvider.GetDailyYieldRate(rateContext);

            foreach (var position in positions)
            {
                if (!position.IsActiveOn(businessDay))
                {
                    continue;
                }

                position.ApplyDailyYield(dailyYieldRate);
                position.UpdateCalendarDaysInvested(businessDay);
            }

            afterBusinessDay?.Invoke(businessDay, positions, rateContext);
        }

        // Ensure days invested reflect the redemption date even when it is not a business day.
        foreach (var position in positions)
        {
            if (position.Date <= endDate)
            {
                position.UpdateCalendarDaysInvested(endDate);
            }
        }

        return new DailyCalculationResult(positions, rateSwitches);
    }

    private static List<ContributionPosition> CreatePositions(Simulation simulation)
    {
        var positions = new List<ContributionPosition>(1 + simulation.Contributions.Count);

        if (simulation.InitialAmount > 0m)
        {
            positions.Add(new ContributionPosition(
                simulation.InitialContributionDate,
                simulation.InitialAmount));
        }

        foreach (var contribution in simulation.Contributions)
        {
            positions.Add(new ContributionPosition(contribution.Date, contribution.Amount));
        }

        return positions;
    }
}
