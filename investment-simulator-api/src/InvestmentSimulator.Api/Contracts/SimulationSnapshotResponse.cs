using InvestmentSimulator.Domain.Enums;

namespace InvestmentSimulator.Api.Contracts;

/// <summary>Saved simulation inputs embedded in a history entry response.</summary>
public sealed class SimulationSnapshotResponse
{
    public InvestmentType Type { get; init; }

    public decimal InitialAmount { get; init; }

    public DateOnly InitialContributionDate { get; init; }

    public DateOnly EndDate { get; init; }

    public IReadOnlyList<ContributionRequest> Contributions { get; init; } = [];

    public IReadOnlyList<AnnualRateRequest> AnnualRates { get; init; } = [];

    public IReadOnlyList<AnnualRateRequest> IpcaRates { get; init; } = [];

    public decimal ProfitabilityPercentage { get; init; }

    public decimal Costs { get; init; }
}
