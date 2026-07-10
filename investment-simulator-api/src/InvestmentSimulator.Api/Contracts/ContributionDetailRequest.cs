namespace InvestmentSimulator.Api.Contracts;

/// <summary>Per-contribution detail used when exporting an existing result.</summary>
public sealed class ContributionDetailRequest
{
    public DateOnly Date { get; init; }

    public decimal Amount { get; init; }

    public decimal Balance { get; init; }

    public decimal Yield { get; init; }

    public int DaysInvested { get; init; }

    public decimal IncomeTax { get; init; }

    public decimal Iof { get; init; }
}
