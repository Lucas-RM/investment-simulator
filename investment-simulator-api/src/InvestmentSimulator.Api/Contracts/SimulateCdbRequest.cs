namespace InvestmentSimulator.Api.Contracts;

/// <summary>Corpo da requisição de <c>POST /simular/cdb</c>.</summary>
public sealed class SimulateCdbRequest
{
    /// <summary>
    /// Valor inicial do investimento em R$. Pode ser zero quando houver aportes adicionais.
    /// </summary>
    public decimal InitialAmount { get; init; }

    /// <summary>Data inicial da simulação (data do aporte inicial).</summary>
    public DateOnly StartDate { get; init; }

    /// <summary>Data de resgate (fim da simulação).</summary>
    public DateOnly EndDate { get; init; }

    /// <summary>Lista de aportes adicionais durante o período.</summary>
    public IReadOnlyList<ContributionRequest> Contributions { get; init; } = [];

    /// <summary>
    /// Taxas anuais do CDI em percentual (ex.: 14.15 = 14,15% a.a.).
    /// Uma única taxa é expandida para todos os anos do período.
    /// </summary>
    public IReadOnlyList<AnnualRateRequest> CdiAnnualRates { get; init; } = [];

    /// <summary>
    /// Taxas anuais do IPCA em percentual (ex.: 4.10 = 4,10% a.a.).
    /// </summary>
    public IReadOnlyList<AnnualRateRequest> IpcaRates { get; init; } = [];

    /// <summary>
    /// Percentual do CDI contratado como multiplicador (ex.: 1.20 = 120% do CDI).
    /// </summary>
    public decimal CdiPercentage { get; init; }
}
