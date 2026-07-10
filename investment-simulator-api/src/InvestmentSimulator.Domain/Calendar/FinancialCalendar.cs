using InvestmentSimulator.Domain.Enums;

namespace InvestmentSimulator.Domain.Calendar;

/// <summary>
/// Financial calendar rules for business days, holidays, contribution date
/// adjustment, and day-count conventions (ERS section 29).
/// <list type="bullet">
/// <item>Business days: Monday–Friday excluding national holidays; 252 per year for rate conversion.</item>
/// <item>Calendar (elapsed) days: used for IR/IOF brackets.</item>
/// <item>Business days: used for daily profitability accrual.</item>
/// </list>
/// </summary>
public sealed class FinancialCalendar
{
    /// <summary>Business days per year used for annual-to-daily rate conversion (ERS section 8).</summary>
    public const int BusinessDaysPerYear = 252;

    private readonly Func<DateOnly, bool> _isHoliday;
    private readonly NonBusinessDayContributionRule _contributionRule;

    /// <summary>
    /// Creates a calendar with Brazilian national holidays by default.
    /// </summary>
    /// <param name="contributionRule">
    /// Rule applied when a contribution falls on a non-business day.
    /// Defaults to postponing to the next business day.
    /// </param>
    /// <param name="isHoliday">
    /// Optional holiday predicate. Defaults to <see cref="BrazilianNationalHolidays.IsHoliday"/>.
    /// </param>
    public FinancialCalendar(
        NonBusinessDayContributionRule contributionRule = NonBusinessDayContributionRule.PostponeToNextBusinessDay,
        Func<DateOnly, bool>? isHoliday = null)
    {
        if (!Enum.IsDefined(contributionRule))
        {
            throw new ArgumentOutOfRangeException(nameof(contributionRule), "Contribution rule must be a valid value.");
        }

        _contributionRule = contributionRule;
        _isHoliday = isHoliday ?? BrazilianNationalHolidays.IsHoliday;
    }

    /// <summary>Rule used by <see cref="AdjustContributionDate"/>.</summary>
    public NonBusinessDayContributionRule ContributionRule => _contributionRule;

    /// <summary>Returns whether the date is Saturday or Sunday.</summary>
    public static bool IsWeekend(DateOnly date) =>
        date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    /// <summary>Returns whether the date is a configured national holiday.</summary>
    public bool IsHoliday(DateOnly date) => _isHoliday(date);

    /// <summary>
    /// Returns whether the date is a business day (weekday and not a holiday).
    /// </summary>
    public bool IsBusinessDay(DateOnly date) =>
        !IsWeekend(date) && !IsHoliday(date);

    /// <summary>
    /// Returns the first business day strictly after <paramref name="date"/>.
    /// </summary>
    public DateOnly GetNextBusinessDay(DateOnly date)
    {
        var candidate = date.AddDays(1);
        while (!IsBusinessDay(candidate))
        {
            candidate = candidate.AddDays(1);
        }

        return candidate;
    }

    /// <summary>
    /// Returns <paramref name="date"/> if it is a business day; otherwise the next business day.
    /// </summary>
    public DateOnly GetSameOrNextBusinessDay(DateOnly date) =>
        IsBusinessDay(date) ? date : GetNextBusinessDay(date);

    /// <summary>
    /// Applies the configured <see cref="ContributionRule"/> to a contribution date.
    /// </summary>
    public DateOnly AdjustContributionDate(DateOnly date) =>
        _contributionRule switch
        {
            NonBusinessDayContributionRule.PostponeToNextBusinessDay => GetSameOrNextBusinessDay(date),
            NonBusinessDayContributionRule.KeepOriginalDate => date,
            _ => throw new InvalidOperationException($"Unsupported contribution rule: {_contributionRule}."),
        };

    /// <summary>
    /// Counts calendar (elapsed) days between two dates.
    /// Used for IR/IOF holding-period brackets (ERS sections 15–16 and 29).
    /// </summary>
    /// <returns><c>end - start</c> in whole days.</returns>
    public static int CountCalendarDays(DateOnly start, DateOnly end)
    {
        EnsureStartNotAfterEnd(start, end);
        return end.DayNumber - start.DayNumber;
    }

    /// <summary>
    /// Counts business days in the half-open interval (<paramref name="start"/>, <paramref name="end"/>].
    /// Used for profitability accrual over business days (ERS sections 10 and 29).
    /// </summary>
    public int CountBusinessDays(DateOnly start, DateOnly end)
    {
        EnsureStartNotAfterEnd(start, end);

        var count = 0;
        for (var day = start.AddDays(1); day <= end; day = day.AddDays(1))
        {
            if (IsBusinessDay(day))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Enumerates business days in the half-open interval (<paramref name="start"/>, <paramref name="end"/>].
    /// </summary>
    public IEnumerable<DateOnly> EnumerateBusinessDays(DateOnly start, DateOnly end)
    {
        EnsureStartNotAfterEnd(start, end);

        for (var day = start.AddDays(1); day <= end; day = day.AddDays(1))
        {
            if (IsBusinessDay(day))
            {
                yield return day;
            }
        }
    }

    private static void EnsureStartNotAfterEnd(DateOnly start, DateOnly end)
    {
        if (end < start)
        {
            throw new ArgumentOutOfRangeException(
                nameof(end),
                "End date cannot be earlier than start date.");
        }
    }
}
