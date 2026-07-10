using InvestmentSimulator.Domain.Enums;

namespace InvestmentSimulator.Api.Contracts;

/// <summary>History entry returned by <c>/historico</c> endpoints.</summary>
public sealed class HistoryEntryResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public DateOnly Date { get; init; }

    public InvestmentType Type { get; init; }

    public string Observations { get; init; } = string.Empty;

    public required SimulationSnapshotResponse Simulation { get; init; }
}
