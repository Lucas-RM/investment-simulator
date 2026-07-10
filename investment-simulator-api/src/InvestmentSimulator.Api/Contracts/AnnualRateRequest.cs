namespace InvestmentSimulator.Api.Contracts;

/// <summary>Annual rate for a calendar year (decimal fraction, e.g. 0.15 = 15%).</summary>
public sealed class AnnualRateRequest
{
    public int Year { get; init; }

    public decimal Rate { get; init; }
}
