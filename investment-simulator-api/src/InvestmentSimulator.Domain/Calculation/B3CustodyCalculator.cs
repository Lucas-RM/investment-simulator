using InvestmentSimulator.Domain.Calendar;
using InvestmentSimulator.Domain.Common;
using InvestmentSimulator.Domain.Exceptions;

namespace InvestmentSimulator.Domain.Calculation;

/// <summary>
/// B3 custody fee formulas (ERS section 14).
/// Exempt up to R$10,000; fee applies only to the excess balance.
/// Daily provisioning; collection on the first business day of January/July or at redemption.
/// </summary>
public static class B3CustodyCalculator
{
    /// <summary>Balance up to this amount (inclusive) is exempt from B3 custody.</summary>
    public const decimal ExemptionThreshold = 10_000m;

    /// <summary>
    /// Returns the taxable base: <c>max(0, balance − 10_000)</c>.
    /// </summary>
    public static decimal CalculateTaxableBase(decimal balance)
    {
        if (balance < 0m)
        {
            throw new DomainValidationException("Balance cannot be negative.");
        }

        return balance <= ExemptionThreshold
            ? 0m
            : Math.Round(
                balance - ExemptionThreshold,
                MonetaryPrecision.IntermediateDecimalPlaces,
                MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Computes the daily custody provision for the given balance and B3 daily rate:
    /// <c>taxable base × daily B3 rate</c>. Zero when balance ≤ R$10,000 or rate is zero.
    /// Result is rounded to <see cref="MonetaryPrecision.IntermediateDecimalPlaces"/>.
    /// </summary>
    /// <param name="balance">Current portfolio balance in BRL.</param>
    /// <param name="dailyB3Rate">
    /// B3 custody daily rate as a decimal fraction (from the annual schedule via 252 business days).
    /// Must be greater than or equal to zero.
    /// </param>
    public static decimal CalculateDailyProvision(decimal balance, decimal dailyB3Rate)
    {
        if (dailyB3Rate < 0m)
        {
            throw new DomainValidationException("B3 daily rate cannot be negative.");
        }

        var taxableBase = CalculateTaxableBase(balance);
        if (taxableBase == 0m || dailyB3Rate == 0m)
        {
            return 0m;
        }

        return Math.Round(
            taxableBase * dailyB3Rate,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Returns the first business day of January or July for <paramref name="year"/>
    /// (semiannual B3 collection dates, ERS section 14).
    /// </summary>
    /// <param name="year">Calendar year.</param>
    /// <param name="month">Must be 1 (January) or 7 (July).</param>
    /// <param name="calendar">Financial calendar used to resolve the first business day.</param>
    public static DateOnly GetSemiannualCollectionDate(
        int year,
        int month,
        FinancialCalendar calendar)
    {
        ArgumentNullException.ThrowIfNull(calendar);

        if (year < 1)
        {
            throw new DomainValidationException("Year must be a valid positive calendar year.");
        }

        if (month is not (1 or 7))
        {
            throw new DomainValidationException(
                "Semiannual B3 collection month must be January (1) or July (7).");
        }

        return calendar.GetSameOrNextBusinessDay(new DateOnly(year, month, 1));
    }

    /// <summary>
    /// Returns whether <paramref name="date"/> is a semiannual B3 collection date:
    /// the first business day of January or of July.
    /// </summary>
    public static bool IsSemiannualCollectionDate(DateOnly date, FinancialCalendar calendar)
    {
        ArgumentNullException.ThrowIfNull(calendar);

        if (date.Month is not (1 or 7))
        {
            return false;
        }

        return date == GetSemiannualCollectionDate(date.Year, date.Month, calendar);
    }
}
