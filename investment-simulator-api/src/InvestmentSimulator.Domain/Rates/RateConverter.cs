using InvestmentSimulator.Domain.Calendar;
using InvestmentSimulator.Domain.Common;
using InvestmentSimulator.Domain.Exceptions;

namespace InvestmentSimulator.Domain.Rates;

/// <summary>
/// Converts annual rates to equivalent daily rates using 252 business days (ERS section 8).
/// Formula: daily = (1 + annual)^(1/252) − 1.
/// </summary>
public static class RateConverter
{
    /// <summary>
    /// Converts an annual rate (decimal fraction, e.g. 0.15 for 15%) to the equivalent daily rate.
    /// Result is rounded to <see cref="MonetaryPrecision.IntermediateDecimalPlaces"/>.
    /// </summary>
    /// <param name="annualRate">Annual rate as a decimal fraction. Must be greater than −1.</param>
    /// <returns>Equivalent daily rate as a decimal fraction.</returns>
    public static decimal AnnualToDaily(decimal annualRate)
    {
        if (annualRate <= -1m)
        {
            throw new DomainValidationException(
                "Annual rate must be greater than -1 so that (1 + rate) is positive.");
        }

        // Fractional exponent requires a floating-point power; result is stored as decimal
        // with intermediate precision (ERS section 28).
        var dailyFactor = Math.Pow(
            (double)(1m + annualRate),
            1.0 / FinancialCalendar.BusinessDaysPerYear);

        var dailyRate = (decimal)dailyFactor - 1m;

        return Math.Round(
            dailyRate,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);
    }
}
