using InvestmentSimulator.Domain.Calendar;
using InvestmentSimulator.Domain.Entities;

namespace InvestmentSimulator.Application.Simulations;

/// <summary>
/// Compares two simulations side by side (ERS section 24),
/// running each through <see cref="SimulationService"/> and exposing
/// net amount, IR, costs, profit, return, and inflation-adjusted value.
/// </summary>
public sealed class SimulationComparisonService
{
    private readonly SimulationService _simulationService;

    /// <summary>
    /// Creates the comparison service with an optional shared calendar
    /// (defaults to Brazilian national holidays when omitted).
    /// </summary>
    public SimulationComparisonService(FinancialCalendar? calendar = null)
        : this(new SimulationService(calendar))
    {
    }

    /// <summary>
    /// Creates the comparison service using an existing simulation orchestrator
    /// (useful for tests and DI).
    /// </summary>
    public SimulationComparisonService(SimulationService simulationService)
    {
        ArgumentNullException.ThrowIfNull(simulationService);
        _simulationService = simulationService;
    }

    /// <summary>
    /// Runs both simulations and returns a side-by-side comparison
    /// with the metrics listed in ERS §24.
    /// </summary>
    /// <param name="left">First simulation (e.g. CDB).</param>
    /// <param name="right">Second simulation (e.g. Tesouro Selic).</param>
    /// <param name="leftOptions">Optional parameters for the left simulation.</param>
    /// <param name="rightOptions">Optional parameters for the right simulation.</param>
    public SimulationComparisonResult Compare(
        Simulation left,
        Simulation right,
        SimulationOptions? leftOptions = null,
        SimulationOptions? rightOptions = null)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        var leftResult = _simulationService.Run(left, leftOptions);
        var rightResult = _simulationService.Run(right, rightOptions);

        return new SimulationComparisonResult(
            SimulationComparisonSide.From(left.Type, leftResult),
            SimulationComparisonSide.From(right.Type, rightResult));
    }
}
