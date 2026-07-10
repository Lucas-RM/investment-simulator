using InvestmentSimulator.Domain.Enums;

namespace InvestmentSimulator.Api.Contracts;

/// <summary>Parâmetros da simulação salva embutidos na resposta do histórico.</summary>
public sealed class SimulationSnapshotResponse
{
    /// <summary>Tipo de investimento.</summary>
    public InvestmentType Type { get; init; }

    /// <summary>Valor inicial em R$.</summary>
    public decimal InitialAmount { get; init; }

    /// <summary>Data inicial da simulação.</summary>
    public DateOnly StartDate { get; init; }

    /// <summary>Data de resgate.</summary>
    public DateOnly EndDate { get; init; }

    /// <summary>Aportes adicionais.</summary>
    public IReadOnlyList<ContributionRequest> Contributions { get; init; } = [];

    /// <summary>
    /// Taxas anuais do índice (CDI ou Selic) em percentual, conforme o tipo.
    /// </summary>
    public IReadOnlyList<AnnualRateRequest> IndexAnnualRates { get; init; } = [];

    /// <summary>Taxas anuais do IPCA em percentual.</summary>
    public IReadOnlyList<AnnualRateRequest> IpcaRates { get; init; } = [];

    /// <summary>Percentual do CDI (multiplicador). Relevante para CDB.</summary>
    public decimal CdiPercentage { get; init; }
}
