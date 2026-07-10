using InvestmentSimulator.Domain.Common;
using InvestmentSimulator.Domain.Exceptions;

namespace InvestmentSimulator.Domain.Calculation;

/// <summary>
/// Tesouro Selic yield formulas (ERS section 13).
/// Daily yield rate = (1 + daily Selic) × (1 + daily ágio) − 1.
/// </summary>
public static class TesouroSelicCalculator
{
    /// <summary>
    /// Computes the effective daily yield rate for Tesouro Selic:
    /// <c>(1 + daily Selic) × (1 + daily ágio) − 1</c>.
    /// Result is rounded to <see cref="MonetaryPrecision.IntermediateDecimalPlaces"/>.
    /// </summary>
    /// <param name="dailySelicRate">Daily Selic Over rate as a decimal fraction.</param>
    /// <param name="dailyAgioRate">
    /// Daily premium (positive) or discount (negative) rate as a decimal fraction.
    /// Must be greater than −1.
    /// </param>
    public static decimal CalculateDailyYieldRate(decimal dailySelicRate, decimal dailyAgioRate)
    {
        EnsureRateGreaterThanMinusOne(dailySelicRate, "Daily Selic rate");
        EnsureRateGreaterThanMinusOne(dailyAgioRate, "Daily ágio rate");

        return Math.Round(
            (1m + dailySelicRate) * (1m + dailyAgioRate) - 1m,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Compounds annual Selic Over and ágio/deságio with the same structure as the daily formula:
    /// <c>(1 + annual Selic) × (1 + annual ágio) − 1</c>.
    /// </summary>
    /// <param name="annualSelicRate">Annual Selic Over rate as a decimal fraction (e.g. 0.15 for 15%).</param>
    /// <param name="annualAgioRate">
    /// Annual premium (positive) or discount (negative) rate as a decimal fraction.
    /// Must be greater than −1.
    /// </param>
    public static decimal CalculateEffectiveAnnualRate(decimal annualSelicRate, decimal annualAgioRate)
    {
        EnsureRateGreaterThanMinusOne(annualSelicRate, "Annual Selic rate");
        EnsureRateGreaterThanMinusOne(annualAgioRate, "Annual ágio rate");

        return Math.Round(
            (1m + annualSelicRate) * (1m + annualAgioRate) - 1m,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);
    }

    private static void EnsureRateGreaterThanMinusOne(decimal rate, string fieldName)
    {
        if (rate <= -1m)
        {
            throw new DomainValidationException(
                $"{fieldName} must be greater than -1 so that (1 + rate) is positive.");
        }
    }
}
