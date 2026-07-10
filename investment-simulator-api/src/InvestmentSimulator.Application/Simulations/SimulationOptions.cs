using InvestmentSimulator.Domain.Entities;

namespace InvestmentSimulator.Application.Simulations;

/// <summary>
/// Optional parameters not yet modeled on <see cref="Domain.Entities.Simulation"/>
/// that the orchestrator needs for Tesouro Selic ágio and B3 custody (ERS §§13–14).
/// </summary>
public sealed class SimulationOptions
{
    /// <summary>
    /// Annual ágio/deságio as a decimal fraction for Tesouro Selic
    /// (e.g. 0.001 for +0.1%; negative for deságio). Ignored for CDB. Default 0.
    /// </summary>
    public decimal AnnualAgioRate { get; init; }

    /// <summary>
    /// Optional annual B3 custody rates (Tesouro Selic only). When null or empty, no B3 custody is provisioned.
    /// Ignored for CDB. A single entry is expanded to every year in the simulation period.
    /// </summary>
    public IReadOnlyList<AnnualRate>? B3CustodyRates { get; init; }
}
