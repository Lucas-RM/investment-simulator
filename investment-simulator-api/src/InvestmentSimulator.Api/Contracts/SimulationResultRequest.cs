namespace InvestmentSimulator.Api.Contracts;

/// <summary>Simulation result payload for export (<c>POST /exportar</c>).</summary>
public sealed class SimulationResultRequest
{
    public decimal InitialAmount { get; init; }

    public decimal ContributionsAmount { get; init; }

    public decimal TotalInvested { get; init; }

    public decimal GrossAmount { get; init; }

    public decimal GrossReturn { get; init; }

    public decimal Costs { get; init; }

    public decimal IncomeTax { get; init; }

    public decimal Iof { get; init; }

    public decimal NetAmount { get; init; }

    public decimal NetReturn { get; init; }

    public decimal NetProfit { get; init; }

    public decimal InflationAdjustedAmount { get; init; }

    public IReadOnlyList<ContributionDetailRequest> ContributionDetails { get; init; } = [];
}
