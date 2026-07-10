namespace InvestmentSimulator.Domain.Rates;

/// <summary>
/// Generates the inclusive list of calendar years between two dates (ERS section 7).
/// Used when the user chooses year-by-year rate entry.
/// </summary>
public static class YearGenerator
{
    /// <summary>
    /// Returns every calendar year from <paramref name="startDate"/>.Year
    /// through <paramref name="endDate"/>.Year, inclusive.
    /// </summary>
    /// <example>
    /// Start 10/08/2026, end 15/04/2031 → 2026, 2027, 2028, 2029, 2030, 2031.
    /// </example>
    public static IReadOnlyList<int> Generate(DateOnly startDate, DateOnly endDate)
    {
        if (endDate < startDate)
        {
            throw new ArgumentOutOfRangeException(
                nameof(endDate),
                "End date cannot be earlier than start date.");
        }

        var startYear = startDate.Year;
        var endYear = endDate.Year;
        var years = new List<int>(endYear - startYear + 1);

        for (var year = startYear; year <= endYear; year++)
        {
            years.Add(year);
        }

        return years;
    }
}
