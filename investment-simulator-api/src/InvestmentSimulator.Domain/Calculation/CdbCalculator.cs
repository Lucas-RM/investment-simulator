using InvestmentSimulator.Domain.Common;
using InvestmentSimulator.Domain.Exceptions;

namespace InvestmentSimulator.Domain.Calculation;

/// <summary>
/// CDB post-fixed yield formulas (ERS section 12).
/// Daily yield rate = daily CDI × contracted profitability.
/// </summary>
public static class CdbCalculator
{
    /// <summary>
    /// Computes the effective daily yield rate for a CDB:
    /// <c>daily CDI × profitability</c> (e.g. CDI daily × 1.10 for 110% CDI).
    /// Result is rounded to <see cref="MonetaryPrecision.IntermediateDecimalPlaces"/>.
    /// </summary>
    /// <param name="dailyCdiRate">Daily CDI rate as a decimal fraction.</param>
    /// <param name="profitabilityPercentage">
    /// Contracted profitability relative to CDI (e.g. 1.10 for 110% CDI). Must be greater than zero.
    /// </param>
    public static decimal CalculateDailyYieldRate(decimal dailyCdiRate, decimal profitabilityPercentage)
    {
        if (profitabilityPercentage <= 0m)
        {
            throw new DomainValidationException("Profitability percentage must be greater than zero.");
        }

        return Math.Round(
            dailyCdiRate * profitabilityPercentage,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Computes the effective annual rate illustrated in ERS section 12
    /// (e.g. CDI 15% × 110% = 16.5%).
    /// </summary>
    /// <param name="annualCdiRate">Annual CDI rate as a decimal fraction (e.g. 0.15 for 15%).</param>
    /// <param name="profitabilityPercentage">
    /// Contracted profitability relative to CDI (e.g. 1.10 for 110% CDI). Must be greater than zero.
    /// </param>
    public static decimal CalculateEffectiveAnnualRate(decimal annualCdiRate, decimal profitabilityPercentage)
    {
        if (profitabilityPercentage <= 0m)
        {
            throw new DomainValidationException("Profitability percentage must be greater than zero.");
        }

        return Math.Round(
            annualCdiRate * profitabilityPercentage,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);
    }
}
