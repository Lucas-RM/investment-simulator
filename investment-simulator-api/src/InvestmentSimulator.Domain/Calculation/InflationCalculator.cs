using InvestmentSimulator.Domain.Common;
using InvestmentSimulator.Domain.Entities;
using InvestmentSimulator.Domain.Exceptions;

namespace InvestmentSimulator.Domain.Calculation;

/// <summary>
/// Inflation accumulation and purchasing-power adjustment (ERS sections 17 and 18).
/// Accumulated inflation compounds annual IPCA rates over the actual simulation period
/// (pro-rata for partial years); the inflation-adjusted amount is
/// net value divided by (1 + accumulated inflation).
/// </summary>
public static class InflationCalculator
{
    /// <summary>
    /// Computes accumulated inflation over complete annual rates as a decimal fraction:
    /// <c>∏(1 + rᵢ) − 1</c> (ERS section 17 example with full years).
    /// Empty sequence yields 0. Result is rounded to
    /// <see cref="MonetaryPrecision.IntermediateDecimalPlaces"/>.
    /// </summary>
    /// <param name="annualIpcaRates">
    /// Annual IPCA rates as decimal fractions (e.g. 0.05 for 5%), in chronological order.
    /// Each rate must be ≥ 0.
    /// </param>
    public static decimal CalculateAccumulatedInflation(IEnumerable<decimal> annualIpcaRates)
    {
        ArgumentNullException.ThrowIfNull(annualIpcaRates);

        var factor = 1m;
        var hasAny = false;

        foreach (var rate in annualIpcaRates)
        {
            if (rate < 0m)
            {
                throw new DomainValidationException("Annual IPCA rate cannot be negative.");
            }

            hasAny = true;
            factor *= 1m + rate;
        }

        if (!hasAny)
        {
            return 0m;
        }

        return Math.Round(
            factor - 1m,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Computes accumulated inflation for the half-open period
    /// <c>[startDate, endDate)</c>, applying each year's IPCA pro-rata:
    /// <c>(1 + r)^(daysInYearPortion / daysInYear) − 1</c>, then compounding across years.
    /// Matches calendar-day counting used for IR/IOF (<c>end − start</c>).
    /// </summary>
    public static decimal CalculateAccumulatedInflationForPeriod(
        DateOnly startDate,
        DateOnly endDate,
        IReadOnlyList<AnnualRate> annualIpcaRates)
    {
        ArgumentNullException.ThrowIfNull(annualIpcaRates);

        if (endDate < startDate)
        {
            throw new DomainValidationException(
                "End date cannot be earlier than the start date for inflation calculation.");
        }

        if (endDate == startDate)
        {
            return 0m;
        }

        foreach (var entry in annualIpcaRates)
        {
            if (entry is null)
            {
                throw new DomainValidationException("IPCA rate entry is required.");
            }

            if (entry.Rate < 0m)
            {
                throw new DomainValidationException("Annual IPCA rate cannot be negative.");
            }
        }

        var ratesByYear = annualIpcaRates.ToDictionary(r => r.Year, r => r.Rate);
        var factor = 1m;

        for (var year = startDate.Year; year <= endDate.Year; year++)
        {
            var yearStart = new DateOnly(year, 1, 1);
            var yearEndExclusive = new DateOnly(year + 1, 1, 1);

            var portionStart = startDate > yearStart ? startDate : yearStart;
            var portionEndExclusive = endDate < yearEndExclusive ? endDate : yearEndExclusive;
            var daysInPortion = portionEndExclusive.DayNumber - portionStart.DayNumber;

            if (daysInPortion <= 0)
            {
                continue;
            }

            if (!ratesByYear.TryGetValue(year, out var annualRate))
            {
                throw new DomainValidationException($"No IPCA rate is defined for year {year}.");
            }

            if (annualRate == 0m)
            {
                continue;
            }

            var daysInYear = DateTime.IsLeapYear(year) ? 366 : 365;
            var yearFactor = (decimal)Math.Pow(
                (double)(1m + annualRate),
                (double)daysInPortion / daysInYear);

            factor *= yearFactor;
        }

        return Math.Round(
            factor - 1m,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Adjusts a net amount for purchasing power (ERS section 18):
    /// <c>netAmount ÷ (1 + accumulatedInflation)</c>.
    /// Result is rounded to <see cref="MonetaryPrecision.IntermediateDecimalPlaces"/>.
    /// </summary>
    /// <param name="netAmount">Net amount after taxes and costs in BRL. Must be ≥ 0.</param>
    /// <param name="accumulatedInflation">
    /// Accumulated inflation as a decimal fraction from
    /// <see cref="CalculateAccumulatedInflation"/>. Must be &gt; −1.
    /// </param>
    public static decimal AdjustForPurchasingPower(decimal netAmount, decimal accumulatedInflation)
    {
        if (netAmount < 0m)
        {
            throw new DomainValidationException("Net amount cannot be negative.");
        }

        if (accumulatedInflation <= -1m)
        {
            throw new DomainValidationException(
                "Accumulated inflation must be greater than -1 so that (1 + inflation) is positive.");
        }

        if (netAmount == 0m)
        {
            return 0m;
        }

        if (accumulatedInflation == 0m)
        {
            return Math.Round(
                netAmount,
                MonetaryPrecision.IntermediateDecimalPlaces,
                MidpointRounding.AwayFromZero);
        }

        return Math.Round(
            netAmount / (1m + accumulatedInflation),
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Computes the inflation-adjusted (real) amount from net value and full-year IPCA rates:
    /// accumulates inflation (ERS §17) then applies purchasing-power adjustment (ERS §18).
    /// Prefer <see cref="CalculateInflationAdjustedAmount(decimal, DateOnly, DateOnly, IReadOnlyList{AnnualRate})"/>
    /// when the simulation period may be a partial year.
    /// </summary>
    public static decimal CalculateInflationAdjustedAmount(
        decimal netAmount,
        IEnumerable<decimal> annualIpcaRates)
    {
        var accumulated = CalculateAccumulatedInflation(annualIpcaRates);
        return AdjustForPurchasingPower(netAmount, accumulated);
    }

    /// <summary>
    /// Computes the inflation-adjusted amount using IPCA rates pro-rated over
    /// <c>[startDate, endDate)</c> (partial years included).
    /// </summary>
    public static decimal CalculateInflationAdjustedAmount(
        decimal netAmount,
        DateOnly startDate,
        DateOnly endDate,
        IReadOnlyList<AnnualRate> annualIpcaRates)
    {
        var accumulated = CalculateAccumulatedInflationForPeriod(
            startDate,
            endDate,
            annualIpcaRates);
        return AdjustForPurchasingPower(netAmount, accumulated);
    }
}
