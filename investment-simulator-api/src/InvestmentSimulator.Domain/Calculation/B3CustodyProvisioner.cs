using InvestmentSimulator.Domain.Calendar;
using InvestmentSimulator.Domain.Common;
using InvestmentSimulator.Domain.Exceptions;

namespace InvestmentSimulator.Domain.Calculation;

/// <summary>
/// Tracks daily B3 custody provisioning and collects the accrued amount on
/// semiannual dates (first business day of January/July) or at redemption
/// (ERS section 14).
/// </summary>
public sealed class B3CustodyProvisioner
{
    private readonly FinancialCalendar _calendar;

    public B3CustodyProvisioner(FinancialCalendar calendar)
    {
        ArgumentNullException.ThrowIfNull(calendar);
        _calendar = calendar;
    }

    /// <summary>Amount provisioned since the last collection, not yet charged.</summary>
    public decimal ProvisionedAmount { get; private set; }

    /// <summary>Total amount collected (charged) so far.</summary>
    public decimal TotalCollected { get; private set; }

    /// <summary>
    /// Accrues one business day's custody provision for the given balance and rate.
    /// Does not collect; use <see cref="ProcessBusinessDay"/> to accrue and collect when due.
    /// </summary>
    public void AccrueDaily(decimal balance, decimal dailyB3Rate)
    {
        var dailyProvision = B3CustodyCalculator.CalculateDailyProvision(balance, dailyB3Rate);
        if (dailyProvision == 0m)
        {
            return;
        }

        ProvisionedAmount = Math.Round(
            ProvisionedAmount + dailyProvision,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Accrues the daily provision and, when <paramref name="businessDay"/> is a
    /// semiannual collection date, collects the accrued amount.
    /// </summary>
    /// <returns>Amount collected on this day (0 when it is not a collection date).</returns>
    public decimal ProcessBusinessDay(DateOnly businessDay, decimal balance, decimal dailyB3Rate)
    {
        if (businessDay == default)
        {
            throw new DomainValidationException("Business day is required and must be a valid date.");
        }

        AccrueDaily(balance, dailyB3Rate);

        if (!B3CustodyCalculator.IsSemiannualCollectionDate(businessDay, _calendar))
        {
            return 0m;
        }

        return Collect();
    }

    /// <summary>
    /// Collects any remaining provisioned amount at redemption (ERS section 14).
    /// </summary>
    /// <returns>Amount collected (0 when nothing is provisioned).</returns>
    public decimal CollectOnRedemption() => Collect();

    private decimal Collect()
    {
        if (ProvisionedAmount == 0m)
        {
            return 0m;
        }

        var charged = ProvisionedAmount;
        ProvisionedAmount = 0m;
        TotalCollected = Math.Round(
            TotalCollected + charged,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);

        return charged;
    }
}
