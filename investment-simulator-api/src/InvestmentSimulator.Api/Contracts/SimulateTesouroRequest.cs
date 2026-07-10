namespace InvestmentSimulator.Api.Contracts;

/// <summary>Corpo da requisição de <c>POST /simular/tesouro</c>.</summary>
public sealed class SimulateTesouroRequest
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
    /// Taxas anuais da Selic Over em percentual (ex.: 14.15 = 14,15% a.a.).
    /// Uma única taxa é expandida para todos os anos do período.
    /// </summary>
    public IReadOnlyList<AnnualRateRequest> SelicAnnualRates { get; init; } = [];

    /// <summary>
    /// Taxas anuais do IPCA em percentual (ex.: 4.10 = 4,10% a.a.).
    /// </summary>
    public IReadOnlyList<AnnualRateRequest> IpcaRates { get; init; } = [];

    /// <summary>
    /// Ágio/deságio anual como fração decimal (ex.: 0.001 = +0,1% a.a.; negativo = deságio).
    /// </summary>
    public decimal AnnualAgioRate { get; init; }

    /// <summary>
    /// Taxas anuais de custódia B3 em percentual (ex.: 0.2 = 0,2% a.a.).
    /// Omitido ou vazio: não aplica custódia B3.
    /// </summary>
    public IReadOnlyList<AnnualRateRequest>? B3CustodyRates { get; init; }
}
