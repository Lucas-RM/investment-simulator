using InvestmentSimulator.Domain.Exceptions;

namespace InvestmentSimulator.Domain.Entities;

/// <summary>
/// Annual rate associated with a calendar year (ERS sections 3 and 6).
/// Rate is stored as a decimal fraction (e.g. 0.15 for 15%).
/// Validated per ERS section 27.
/// </summary>
public sealed class AnnualRate
{
    public AnnualRate(int year, decimal rate)
    {
        if (year < 1)
        {
            throw new DomainValidationException("Year must be a valid positive calendar year.");
        }

        if (rate < 0m)
        {
            throw new DomainValidationException("Annual rate cannot be negative.");
        }

        Year = year;
        Rate = rate;
    }

    /// <summary>Calendar year the rate applies to.</summary>
    public int Year { get; }

    /// <summary>Annual rate as a decimal fraction (e.g. 0.15 for 15%).</summary>
    public decimal Rate { get; }
}
