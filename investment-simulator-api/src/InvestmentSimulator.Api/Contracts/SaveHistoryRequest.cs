using InvestmentSimulator.Domain.Enums;

namespace InvestmentSimulator.Api.Contracts;

/// <summary>Corpo da requisição de <c>POST /historico</c>.</summary>
public sealed class SaveHistoryRequest
{
    /// <summary>Nome da simulação salva.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Data de registro no histórico.</summary>
    public DateOnly Date { get; init; }

    /// <summary>Observações livres (pode ser vazia).</summary>
    public string Observations { get; init; } = string.Empty;

    /// <summary>Identificador opcional para sobrescrever uma entrada existente.</summary>
    public Guid? Id { get; init; }

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
    /// Taxas anuais do CDI em percentual. Usado quando <c>Type</c> = <c>Cdb</c>.
    /// </summary>
    public IReadOnlyList<AnnualRateRequest> CdiAnnualRates { get; init; } = [];

    /// <summary>
    /// Taxas anuais da Selic Over em percentual. Usado quando <c>Type</c> = <c>TesouroSelic</c>.
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
}
