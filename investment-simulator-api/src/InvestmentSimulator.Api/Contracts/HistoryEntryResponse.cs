using InvestmentSimulator.Domain.Enums;

namespace InvestmentSimulator.Api.Contracts;

/// <summary>Entrada do histórico retornada por <c>/historico</c>.</summary>
public sealed class HistoryEntryResponse
{
    /// <summary>Identificador único da entrada.</summary>
    public Guid Id { get; init; }

    /// <summary>Nome da simulação salva.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Data de registro no histórico.</summary>
    public DateOnly Date { get; init; }

    /// <summary>Tipo de investimento.</summary>
    public InvestmentType Type { get; init; }

    /// <summary>Observações livres.</summary>
    public string Observations { get; init; } = string.Empty;

    /// <summary>Parâmetros da simulação salva.</summary>
    public required SimulationSnapshotResponse Simulation { get; init; }
}
