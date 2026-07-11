namespace InvestmentSimulator.Api.Contracts;

/// <summary>Payload de resultado para exportação (<c>POST /exportar</c>).</summary>
public sealed class SimulationResultRequest
{
    /// <summary>Data inicial da simulação.</summary>
    public DateOnly StartDate { get; init; }

    /// <summary>Data de resgate.</summary>
    public DateOnly EndDate { get; init; }

    /// <summary>Valor inicial investido em R$.</summary>
    public decimal InitialAmount { get; init; }

    /// <summary>Soma dos aportes adicionais em R$.</summary>
    public decimal TotalAdditionalContributions { get; init; }

    /// <summary>Total investido em R$.</summary>
    public decimal TotalInvested { get; init; }

    /// <summary>Valor bruto em R$.</summary>
    public decimal GrossAmount { get; init; }

    /// <summary>Rentabilidade bruta (fração decimal).</summary>
    public decimal GrossReturnPercentage { get; init; }

    /// <summary>Lucro bruto total em R$.</summary>
    public decimal TotalGrossYield { get; init; }

    /// <summary>Custos totais em R$.</summary>
    public decimal Costs { get; init; }

    /// <summary>Total de IR em R$.</summary>
    public decimal IncomeTax { get; init; }

    /// <summary>Total de IOF em R$.</summary>
    public decimal Iof { get; init; }

    /// <summary>Valor líquido em R$.</summary>
    public decimal NetAmount { get; init; }

    /// <summary>Rentabilidade líquida (fração decimal).</summary>
    public decimal NetReturnPercentage { get; init; }

    /// <summary>Lucro líquido total em R$.</summary>
    public decimal TotalNetYield { get; init; }

    /// <summary>Valor líquido ajustado pela inflação em R$.</summary>
    public decimal NetAmountInflationAdjusted { get; init; }

    /// <summary>Detalhamento por aporte.</summary>
    public IReadOnlyList<ContributionDetailRequest> ContributionDetails { get; init; } = [];
}
