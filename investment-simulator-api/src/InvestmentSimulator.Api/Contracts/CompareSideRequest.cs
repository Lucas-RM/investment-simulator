using InvestmentSimulator.Domain.Enums;

namespace InvestmentSimulator.Api.Contracts;

/// <summary>Um lado da comparação (<c>POST /comparar</c>).</summary>
public sealed class CompareSideRequest
{
    /// <summary>Tipo de investimento: <c>Cdb</c> ou <c>TesouroSelic</c>.</summary>
    public InvestmentType Type { get; init; }

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
    /// Taxas anuais do CDI em percentual. Obrigatório quando <c>Type</c> = <c>Cdb</c>.
    /// </summary>
    public IReadOnlyList<AnnualRateRequest> CdiAnnualRates { get; init; } = [];

    /// <summary>
    /// Taxas anuais da Selic Over em percentual. Obrigatório quando <c>Type</c> = <c>TesouroSelic</c>.
    /// </summary>
    public IReadOnlyList<AnnualRateRequest> SelicAnnualRates { get; init; } = [];

    /// <summary>
    /// Taxas anuais do IPCA em percentual (ex.: 4.10 = 4,10% a.a.).
    /// </summary>
    public IReadOnlyList<AnnualRateRequest> IpcaRates { get; init; } = [];

    /// <summary>
    /// Percentual do CDI (ex.: 1.10 = 110% do CDI). Obrigatório para CDB.
    /// Para Tesouro Selic, usa 1 quando omitido/zero.
    /// </summary>
    public decimal CdiPercentage { get; init; }

    /// <summary>Ágio/deságio anual (fração decimal) para Tesouro Selic. Ignorado no CDB.</summary>
    public decimal AnnualAgioRate { get; init; }

    /// <summary>
    /// Taxas anuais de custódia B3 em percentual. Aplicável apenas ao Tesouro Selic.
    /// </summary>
    public IReadOnlyList<AnnualRateRequest>? B3CustodyRates { get; init; }
}
