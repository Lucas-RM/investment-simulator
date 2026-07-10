namespace InvestmentSimulator.Domain.Results;

/// <summary>
/// Per-contribution calculation detail treated as an independent investment
/// (ERS sections 9 and 20). Monetary values use <see cref="decimal"/> (ERS section 28).
/// </summary>
public sealed class ContributionDetail
{
    public ContributionDetail(
        DateOnly date,
        decimal amount,
        decimal balance,
        decimal yield,
        int daysInvested,
        decimal incomeTax,
        decimal iof)
    {
        Date = date;
        Amount = amount;
        Balance = balance;
        Yield = yield;
        DaysInvested = daysInvested;
        IncomeTax = incomeTax;
        Iof = iof;
    }

    /// <summary>Contribution date.</summary>
    public DateOnly Date { get; }

    /// <summary>Initial contribution amount in BRL.</summary>
    public decimal Amount { get; }

    /// <summary>Current balance of this contribution in BRL.</summary>
    public decimal Balance { get; }

    /// <summary>Accumulated yield (rendimento) of this contribution in BRL.</summary>
    public decimal Yield { get; }

    /// <summary>Number of calendar days invested (used for IR/IOF).</summary>
    public int DaysInvested { get; }

    /// <summary>Income tax (IR) amount for this contribution in BRL.</summary>
    public decimal IncomeTax { get; }

    /// <summary>IOF amount for this contribution in BRL.</summary>
    public decimal Iof { get; }
}
