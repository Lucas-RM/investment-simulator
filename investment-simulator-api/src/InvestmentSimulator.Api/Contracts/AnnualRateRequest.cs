namespace InvestmentSimulator.Api.Contracts;

/// <summary>Taxa anual para um ano-calendário.</summary>
public sealed class AnnualRateRequest
{
    /// <summary>Ano-calendário da taxa (ex.: 2026).</summary>
    public int Year { get; init; }

    /// <summary>
    /// Taxa anual em percentual (ex.: 14.15 = 14,15% a.a.; 4.10 = 4,10% a.a.).
    /// </summary>
    public decimal Rate { get; init; }
}
