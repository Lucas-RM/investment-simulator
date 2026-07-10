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
        decimal grossBalance,
        decimal grossYield,
        int calendarDaysInvested,
        int businessDaysInvested,
        decimal incomeTax,
        decimal iof)
    {
        Date = date;
        Amount = amount;
        GrossBalance = grossBalance;
        GrossYield = grossYield;
        CalendarDaysInvested = calendarDaysInvested;
        BusinessDaysInvested = businessDaysInvested;
        IncomeTax = incomeTax;
        Iof = iof;
    }

    /// <summary>Contribution date.</summary>
    public DateOnly Date { get; }

    /// <summary>Initial contribution amount in BRL.</summary>
    public decimal Amount { get; }

    /// <summary>Current gross balance of this contribution in BRL.</summary>
    public decimal GrossBalance { get; }

    /// <summary>Accumulated gross yield (rendimento) of this contribution in BRL.</summary>
    public decimal GrossYield { get; }

    /// <summary>Number of calendar days invested (used for IR/IOF).</summary>
    public int CalendarDaysInvested { get; }

    /// <summary>Number of business days on which yield was applied.</summary>
    public int BusinessDaysInvested { get; }

    /// <summary>Income tax (IR) amount for this contribution in BRL.</summary>
    public decimal IncomeTax { get; }

    /// <summary>IOF amount for this contribution in BRL.</summary>
    public decimal Iof { get; }
}
