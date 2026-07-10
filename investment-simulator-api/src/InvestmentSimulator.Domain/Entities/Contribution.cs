using InvestmentSimulator.Domain.Exceptions;

namespace InvestmentSimulator.Domain.Entities;

/// <summary>
/// A single contribution (aporte) with date and amount (ERS section 4).
/// Monetary values use <see cref="decimal"/> (ERS section 28).
/// Validated per ERS sections 5 and 27.
/// </summary>
public sealed class Contribution
{
    public Contribution(DateOnly date, decimal amount)
    {
        if (date == default)
        {
            throw new DomainValidationException("Contribution date is required and must be a valid date.");
        }

        if (amount <= 0m)
        {
            throw new DomainValidationException("Contribution amount must be greater than zero.");
        }

        Date = date;
        Amount = amount;
    }

    /// <summary>Contribution date.</summary>
    public DateOnly Date { get; }

    /// <summary>Contribution amount in BRL.</summary>
    public decimal Amount { get; }
}
