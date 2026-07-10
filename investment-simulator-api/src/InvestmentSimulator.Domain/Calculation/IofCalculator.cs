using InvestmentSimulator.Domain.Common;
using InvestmentSimulator.Domain.Exceptions;

namespace InvestmentSimulator.Domain.Calculation;

/// <summary>
/// IOF (Imposto sobre Operações Financeiras) formulas for fixed-income redemptions
/// (ERS section 15).
/// Applies only when calendar days invested are less than 30, only on yield (rendimento),
/// using the official regressive rate table.
/// </summary>
public static class IofCalculator
{
    /// <summary>
    /// Holding period (calendar days) from which IOF is fully exempt (rate = 0).
    /// IOF applies only when <c>daysInvested &lt; ExemptionDays</c>.
    /// </summary>
    public const int ExemptionDays = 30;

    /// <summary>
    /// Official regressive IOF rates by calendar day invested (1–29).
    /// Index equals the number of days; index 0 is unused.
    /// Source: Decreto nº 6.306/2007 (fixed-income operations).
    /// </summary>
    private static readonly decimal[] RatesByDay =
    [
        0.00m, // day 0 — no holding period
        0.96m, // 1
        0.93m, // 2
        0.90m, // 3
        0.86m, // 4
        0.83m, // 5
        0.80m, // 6
        0.76m, // 7
        0.73m, // 8
        0.70m, // 9
        0.66m, // 10
        0.63m, // 11
        0.60m, // 12
        0.56m, // 13
        0.53m, // 14
        0.50m, // 15
        0.46m, // 16
        0.43m, // 17
        0.40m, // 18
        0.36m, // 19
        0.33m, // 20
        0.30m, // 21
        0.26m, // 22
        0.23m, // 23
        0.20m, // 24
        0.16m, // 25
        0.13m, // 26
        0.10m, // 27
        0.06m, // 28
        0.03m, // 29
    ];

    /// <summary>
    /// Returns whether IOF is exempt for the given holding period
    /// (<paramref name="daysInvested"/> ≥ <see cref="ExemptionDays"/>).
    /// </summary>
    public static bool IsExempt(int daysInvested)
    {
        if (daysInvested < 0)
        {
            throw new DomainValidationException("Days invested cannot be negative.");
        }

        return daysInvested >= ExemptionDays;
    }

    /// <summary>
    /// Returns the IOF rate (decimal fraction) for the given calendar days invested.
    /// Zero when <paramref name="daysInvested"/> is 0 or ≥ <see cref="ExemptionDays"/>.
    /// </summary>
    public static decimal GetRate(int daysInvested)
    {
        if (daysInvested < 0)
        {
            throw new DomainValidationException("Days invested cannot be negative.");
        }

        if (daysInvested == 0 || daysInvested >= ExemptionDays)
        {
            return 0m;
        }

        return RatesByDay[daysInvested];
    }

    /// <summary>
    /// Computes IOF due on redemption: <c>yield × rate(daysInvested)</c>.
    /// Zero when exempt (≥ 30 days), when there is no yield, or when days invested is 0.
    /// Result is rounded to <see cref="MonetaryPrecision.IntermediateDecimalPlaces"/>.
    /// </summary>
    /// <param name="yield">Accumulated yield (rendimento) of the contribution in BRL. Must be ≥ 0.</param>
    /// <param name="daysInvested">Calendar days invested (ERS sections 15 and 29). Must be ≥ 0.</param>
    public static decimal Calculate(decimal yield, int daysInvested)
    {
        if (yield < 0m)
        {
            throw new DomainValidationException("Yield cannot be negative.");
        }

        if (daysInvested < 0)
        {
            throw new DomainValidationException("Days invested cannot be negative.");
        }

        if (yield == 0m || IsExempt(daysInvested))
        {
            return 0m;
        }

        var rate = GetRate(daysInvested);
        if (rate == 0m)
        {
            return 0m;
        }

        return Math.Round(
            yield * rate,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);
    }
}
