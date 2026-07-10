namespace InvestmentSimulator.Domain.Calendar;

/// <summary>
/// Brazilian national holidays used by the financial calendar (ERS section 29).
/// Includes fixed dates and movable holidays based on Easter (ANBIMA-style).
/// Dia da Consciência Negra (20 Nov) is treated as a national holiday from 2024 onward.
/// </summary>
public static class BrazilianNationalHolidays
{
    /// <summary>
    /// Returns whether <paramref name="date"/> is a Brazilian national holiday.
    /// </summary>
    public static bool IsHoliday(DateOnly date)
    {
        return GetHolidaysForYear(date.Year).Contains(date);
    }

    /// <summary>
    /// Returns all national holidays for the given year.
    /// </summary>
    public static IReadOnlySet<DateOnly> GetHolidaysForYear(int year)
    {
        var easter = ComputeEasterSunday(year);

        var holidays = new HashSet<DateOnly>
        {
            new(year, 1, 1),   // Confraternização Universal
            easter.AddDays(-48), // Carnival Monday
            easter.AddDays(-47), // Carnival Tuesday
            easter.AddDays(-2),  // Good Friday
            new(year, 4, 21),  // Tiradentes
            new(year, 5, 1),   // Labour Day
            easter.AddDays(60),  // Corpus Christi
            new(year, 9, 7),   // Independence Day
            new(year, 10, 12), // Our Lady of Aparecida
            new(year, 11, 2),  // All Souls' Day
            new(year, 11, 15), // Proclamation of the Republic
            new(year, 12, 25), // Christmas
        };

        // Lei 14.759/2023 — national holiday from 2024.
        if (year >= 2024)
        {
            holidays.Add(new DateOnly(year, 11, 20));
        }

        return holidays;
    }

    /// <summary>
    /// Returns national holidays for every year in [<paramref name="startYear"/>, <paramref name="endYear"/>].
    /// </summary>
    public static IReadOnlySet<DateOnly> GetHolidays(int startYear, int endYear)
    {
        if (endYear < startYear)
        {
            throw new ArgumentOutOfRangeException(
                nameof(endYear),
                "End year cannot be earlier than start year.");
        }

        var holidays = new HashSet<DateOnly>();
        for (var year = startYear; year <= endYear; year++)
        {
            holidays.UnionWith(GetHolidaysForYear(year));
        }

        return holidays;
    }

    /// <summary>
    /// Anonymous Gregorian algorithm (Meeus/Jones/Butcher) for Easter Sunday.
    /// </summary>
    public static DateOnly ComputeEasterSunday(int year)
    {
        var a = year % 19;
        var b = year / 100;
        var c = year % 100;
        var d = b / 4;
        var e = b % 4;
        var f = (b + 8) / 25;
        var g = (b - f + 1) / 3;
        var h = ((19 * a) + b - d - g + 15) % 30;
        var i = c / 4;
        var k = c % 4;
        var l = (32 + (2 * e) + (2 * i) - h - k) % 7;
        var m = (a + (11 * h) + (22 * l)) / 451;
        var month = (h + l - (7 * m) + 114) / 31;
        var day = ((h + l - (7 * m) + 114) % 31) + 1;

        return new DateOnly(year, month, day);
    }
}
