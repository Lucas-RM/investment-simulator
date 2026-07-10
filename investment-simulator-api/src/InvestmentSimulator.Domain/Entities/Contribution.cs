namespace InvestmentSimulator.Domain.Entities;

/// <summary>
/// A single contribution (aporte) with date and amount (ERS section 4).
/// Monetary values use <see cref="decimal"/> (ERS section 28).
/// </summary>
public sealed class Contribution
{
    public Contribution(DateOnly date, decimal amount)
    {
        Date = date;
        Amount = amount;
    }

    /// <summary>Contribution date.</summary>
    public DateOnly Date { get; }

    /// <summary>Contribution amount in BRL.</summary>
    public decimal Amount { get; }
}
