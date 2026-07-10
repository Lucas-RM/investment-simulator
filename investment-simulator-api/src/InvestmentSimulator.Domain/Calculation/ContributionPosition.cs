using InvestmentSimulator.Domain.Common;
using InvestmentSimulator.Domain.Exceptions;
using InvestmentSimulator.Domain.Results;

namespace InvestmentSimulator.Domain.Calculation;

/// <summary>
/// Mutable state of a single contribution treated as an independent investment
/// during the daily calculation loop (ERS section 9).
/// Tracks initial amount, current balance, yield, days invested, IR and IOF.
/// </summary>
public sealed class ContributionPosition
{
    public ContributionPosition(DateOnly date, decimal initialAmount)
    {
        if (date == default)
        {
            throw new DomainValidationException("Contribution date is required and must be a valid date.");
        }

        if (initialAmount <= 0m)
        {
            throw new DomainValidationException("Contribution initial amount must be greater than zero.");
        }

        Date = date;
        InitialAmount = initialAmount;
        Balance = initialAmount;
        Yield = 0m;
        DaysInvested = 0;
        IncomeTax = 0m;
        Iof = 0m;
    }

    /// <summary>Contribution (start) date.</summary>
    public DateOnly Date { get; }

    /// <summary>Initial contribution amount in BRL.</summary>
    public decimal InitialAmount { get; }

    /// <summary>Current balance of this contribution in BRL.</summary>
    public decimal Balance { get; private set; }

    /// <summary>Accumulated yield (rendimento) of this contribution in BRL.</summary>
    public decimal Yield { get; private set; }

    /// <summary>Calendar days invested (base for IR/IOF, ERS sections 15–16 and 29).</summary>
    public int DaysInvested { get; private set; }

    /// <summary>Income tax (IR) amount in BRL. Populated by later tax calculation steps.</summary>
    public decimal IncomeTax { get; private set; }

    /// <summary>IOF amount in BRL. Populated by later tax calculation steps.</summary>
    public decimal Iof { get; private set; }

    /// <summary>
    /// Returns whether this contribution is active on <paramref name="businessDay"/>:
    /// accrual uses the half-open interval (contribution date, business day]
    /// (ERS sections 10 and 29).
    /// </summary>
    public bool IsActiveOn(DateOnly businessDay) => Date < businessDay;

    /// <summary>
    /// Applies one business day's yield using the effective daily rate (ERS section 10).
    /// Updates balance and accumulated yield with intermediate precision (ERS section 28).
    /// </summary>
    public void ApplyDailyYield(decimal dailyRate)
    {
        var dailyYield = Math.Round(
            Balance * dailyRate,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);

        Balance = Math.Round(
            Balance + dailyYield,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);

        Yield = Math.Round(
            Yield + dailyYield,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Sets calendar days invested as of <paramref name="asOfDate"/> (ERS section 10).
    /// </summary>
    public void UpdateDaysInvested(DateOnly asOfDate)
    {
        if (asOfDate < Date)
        {
            DaysInvested = 0;
            return;
        }

        DaysInvested = asOfDate.DayNumber - Date.DayNumber;
    }

    /// <summary>
    /// Sets tax amounts. Reserved for IR/IOF calculators (ERS sections 15–16).
    /// </summary>
    public void SetTaxes(decimal incomeTax, decimal iof)
    {
        if (incomeTax < 0m)
        {
            throw new DomainValidationException("Income tax cannot be negative.");
        }

        if (iof < 0m)
        {
            throw new DomainValidationException("IOF cannot be negative.");
        }

        IncomeTax = incomeTax;
        Iof = iof;
    }

    /// <summary>Projects the current state into an immutable <see cref="ContributionDetail"/>.</summary>
    public ContributionDetail ToDetail() =>
        new(
            Date,
            InitialAmount,
            Balance,
            Yield,
            DaysInvested,
            IncomeTax,
            Iof);
}
