namespace InvestmentSimulator.Api.Contracts;

/// <summary>Resumo final da simulação (status 200).</summary>
public sealed class SimulationResultResponse
{
    /// <summary>Valor inicial investido em R$.</summary>
    public decimal InitialAmount { get; init; }

    /// <summary>Soma dos aportes adicionais em R$.</summary>
    public decimal TotalAdditionalContributions { get; init; }

    /// <summary>Total investido (inicial + aportes) em R$.</summary>
    public decimal TotalInvested { get; init; }

    /// <summary>Valor bruto ao resgate (antes de impostos e custos) em R$.</summary>
    public decimal GrossAmount { get; init; }

    /// <summary>
    /// Rentabilidade bruta como fração decimal (ex.: 0.15 = 15%).
    /// </summary>
    public decimal GrossReturnPercentage { get; init; }

    /// <summary>
    /// Custos totais em R$. No CDB é sempre zero; no Tesouro Selic inclui custódia B3.
    /// </summary>
    public decimal Costs { get; init; }

    /// <summary>Total de Imposto de Renda (IR) em R$.</summary>
    public decimal IncomeTax { get; init; }

    /// <summary>Total de IOF em R$.</summary>
    public decimal Iof { get; init; }

    /// <summary>Valor líquido após impostos e custos em R$.</summary>
    public decimal NetAmount { get; init; }

    /// <summary>
    /// Rentabilidade líquida como fração decimal (ex.: 0.12 = 12%).
    /// </summary>
    public decimal NetReturnPercentage { get; init; }

    /// <summary>Lucro líquido total (valor líquido − total investido) em R$.</summary>
    public decimal TotalNetYield { get; init; }

    /// <summary>
    /// Valor líquido ajustado pela inflação (poder de compra) em R$.
    /// </summary>
    public decimal NetAmountInflationAdjusted { get; init; }

    /// <summary>Detalhamento independente de cada aporte.</summary>
    public IReadOnlyList<ContributionDetailResponse> ContributionDetails { get; init; } = [];
}
