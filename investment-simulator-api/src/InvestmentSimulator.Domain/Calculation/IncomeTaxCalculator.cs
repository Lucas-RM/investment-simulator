using InvestmentSimulator.Domain.Common;
using InvestmentSimulator.Domain.Exceptions;

namespace InvestmentSimulator.Domain.Calculation;

/// <summary>
/// Income tax (IR) formulas for fixed-income redemptions (ERS section 16).
/// Applied individually per contribution using the official regressive rate table
/// based on calendar days invested.
/// </summary>
public static class IncomeTaxCalculator
{
    /// <summary>Rate for holdings up to 180 calendar days (22.5%).</summary>
    public const decimal RateUpTo180Days = 0.225m;

    /// <summary>Rate for holdings from 181 to 360 calendar days (20%).</summary>
    public const decimal RateFrom181To360Days = 0.20m;

    /// <summary>Rate for holdings from 361 to 720 calendar days (17.5%).</summary>
    public const decimal RateFrom361To720Days = 0.175m;

    /// <summary>Rate for holdings above 720 calendar days (15%).</summary>
    public const decimal RateAbove720Days = 0.15m;

    /// <summary>
    /// Returns the IR rate (decimal fraction) for the given calendar days invested.
    /// <list type="bullet">
    /// <item>≤ 180 days → 22.5%</item>
    /// <item>181–360 days → 20%</item>
    /// <item>361–720 days → 17.5%</item>
    /// <item>&gt; 720 days → 15%</item>
    /// </list>
    /// </summary>
    public static decimal GetRate(int daysInvested)
    {
        if (daysInvested < 0)
        {
            throw new DomainValidationException("Days invested cannot be negative.");
        }

        if (daysInvested <= 180)
        {
            return RateUpTo180Days;
        }

        if (daysInvested <= 360)
        {
            return RateFrom181To360Days;
        }

        if (daysInvested <= 720)
        {
            return RateFrom361To720Days;
        }

        return RateAbove720Days;
    }

    /// <summary>
    /// Computes IR due on redemption for a single contribution:
    /// <c>yield × rate(daysInvested)</c>.
    /// Zero when there is no yield. Result is rounded to
    /// <see cref="MonetaryPrecision.IntermediateDecimalPlaces"/>.
    /// </summary>
    /// <param name="yield">
    /// Taxable yield (rendimento) of the contribution in BRL.
    /// When IOF also applies, pass yield after IOF deduction. Must be ≥ 0.
    /// </param>
    /// <param name="daysInvested">Calendar days invested (ERS sections 16 and 29). Must be ≥ 0.</param>
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

        if (yield == 0m)
        {
            return 0m;
        }

        return Math.Round(
            yield * GetRate(daysInvested),
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);
    }
}
