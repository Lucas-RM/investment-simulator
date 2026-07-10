using InvestmentSimulator.Domain.Entities;
using InvestmentSimulator.Domain.Exceptions;

namespace InvestmentSimulator.Domain.Rates;

/// <summary>
/// Annual rate schedule for a date range, supporting a single rate for all years
/// or one rate per year (ERS sections 6 and 7).
/// </summary>
public sealed class RateSchedule
{
    private readonly Dictionary<int, decimal> _ratesByYear;

    private RateSchedule(RateEntryMode mode, IReadOnlyList<AnnualRate> rates)
    {
        Mode = mode;
        Rates = rates;
        _ratesByYear = rates.ToDictionary(r => r.Year, r => r.Rate);
    }

    /// <summary>Whether rates were entered as a single value or year by year.</summary>
    public RateEntryMode Mode { get; }

    /// <summary>Resolved annual rates covering every year in the period.</summary>
    public IReadOnlyList<AnnualRate> Rates { get; }

    /// <summary>
    /// Builds a schedule by applying the same annual rate to every year
    /// between <paramref name="startDate"/> and <paramref name="endDate"/> (ERS section 6).
    /// </summary>
    public static RateSchedule FromSingleRate(decimal rate, DateOnly startDate, DateOnly endDate)
    {
        var years = YearGenerator.Generate(startDate, endDate);
        var rates = years.Select(year => new AnnualRate(year, rate)).ToList();
        return new RateSchedule(RateEntryMode.SingleRate, rates);
    }

    /// <summary>
    /// Builds a schedule from an explicit rate per year.
    /// The list must cover exactly the years between <paramref name="startDate"/>
    /// and <paramref name="endDate"/> with no duplicates (ERS sections 6 and 7).
    /// </summary>
    public static RateSchedule FromPerYear(
        IReadOnlyList<AnnualRate> rates,
        DateOnly startDate,
        DateOnly endDate)
    {
        ArgumentNullException.ThrowIfNull(rates);

        var expectedYears = YearGenerator.Generate(startDate, endDate);

        if (rates.Count != expectedYears.Count)
        {
            throw new DomainValidationException(
                $"Per-year rates must include exactly {expectedYears.Count} entries " +
                $"(years {expectedYears[0]}–{expectedYears[^1]}), but {rates.Count} were provided.");
        }

        var seenYears = new HashSet<int>();

        for (var i = 0; i < rates.Count; i++)
        {
            var entry = rates[i]
                ?? throw new DomainValidationException($"Rate entry at index {i} is required.");

            if (!seenYears.Add(entry.Year))
            {
                throw new DomainValidationException(
                    $"Duplicate rate for year {entry.Year} is not allowed.");
            }
        }

        foreach (var year in expectedYears)
        {
            if (!seenYears.Contains(year))
            {
                throw new DomainValidationException(
                    $"Missing rate for year {year}. All years between the start and end dates are required.");
            }
        }

        var ordered = expectedYears
            .Select(year => rates.First(r => r.Year == year))
            .ToList();

        return new RateSchedule(RateEntryMode.PerYear, ordered);
    }

    /// <summary>Returns the annual rate (decimal fraction) for the given calendar year.</summary>
    public decimal GetAnnualRate(int year)
    {
        if (!_ratesByYear.TryGetValue(year, out var rate))
        {
            throw new DomainValidationException($"No rate is defined for year {year}.");
        }

        return rate;
    }

    /// <summary>
    /// Returns the daily equivalent of the annual rate for the given year (ERS section 8).
    /// </summary>
    public decimal GetDailyRate(int year) =>
        RateConverter.AnnualToDaily(GetAnnualRate(year));
}
