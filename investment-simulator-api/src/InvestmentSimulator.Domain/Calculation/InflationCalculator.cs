using InvestmentSimulator.Domain.Common;
using InvestmentSimulator.Domain.Exceptions;

namespace InvestmentSimulator.Domain.Calculation;

/// <summary>
/// Inflation accumulation and purchasing-power adjustment (ERS sections 17 and 18).
/// Accumulated inflation uses the compound product of annual IPCA rates;
/// the inflation-adjusted amount is net value divided by (1 + accumulated inflation).
/// </summary>
public static class InflationCalculator
{
    /// <summary>
    /// Computes accumulated inflation over the period as a decimal fraction:
    /// <c>∏(1 + rᵢ) − 1</c> (ERS section 17).
    /// Example: 5%, 4%, 4.5% → <c>(1.05 × 1.04 × 1.045) − 1</c>.
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
    /// Computes the inflation-adjusted (real) amount from net value and annual IPCA rates:
    /// accumulates inflation (ERS §17) then applies purchasing-power adjustment (ERS §18).
    /// </summary>
    /// <param name="netAmount">Net amount after taxes and costs in BRL. Must be ≥ 0.</param>
    /// <param name="annualIpcaRates">
    /// Annual IPCA rates as decimal fractions (e.g. 0.05 for 5%), in chronological order.
    /// </param>
    public static decimal CalculateInflationAdjustedAmount(
        decimal netAmount,
        IEnumerable<decimal> annualIpcaRates)
    {
        var accumulated = CalculateAccumulatedInflation(annualIpcaRates);
        return AdjustForPurchasingPower(netAmount, accumulated);
    }
}
