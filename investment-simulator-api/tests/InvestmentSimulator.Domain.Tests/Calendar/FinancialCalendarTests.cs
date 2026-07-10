using InvestmentSimulator.Domain.Calendar;
using InvestmentSimulator.Domain.Enums;

namespace InvestmentSimulator.Domain.Tests.Calendar;

public class BrazilianNationalHolidaysTests
{
    [Theory]
    [InlineData(2024, 3, 31)]
    [InlineData(2025, 4, 20)]
    [InlineData(2026, 4, 5)]
    public void ComputeEasterSunday_ShouldMatchKnownDates(int year, int month, int day)
    {
        Assert.Equal(new DateOnly(year, month, day), BrazilianNationalHolidays.ComputeEasterSunday(year));
    }

    [Theory]
    [InlineData(2026, 1, 1)]   // New Year
    [InlineData(2026, 2, 16)]  // Carnival Monday (Easter 2026-04-05)
    [InlineData(2026, 2, 17)]  // Carnival Tuesday
    [InlineData(2026, 4, 3)]   // Good Friday
    [InlineData(2026, 4, 21)]  // Tiradentes
    [InlineData(2026, 5, 1)]   // Labour Day
    [InlineData(2026, 6, 4)]   // Corpus Christi
    [InlineData(2026, 9, 7)]   // Independence
    [InlineData(2026, 10, 12)] // Our Lady of Aparecida
    [InlineData(2026, 11, 2)]  // All Souls'
    [InlineData(2026, 11, 15)] // Republic
    [InlineData(2026, 11, 20)] // Black Consciousness (from 2024)
    [InlineData(2026, 12, 25)] // Christmas
    public void IsHoliday_ShouldRecognizeNationalHolidays(int year, int month, int day)
    {
        Assert.True(BrazilianNationalHolidays.IsHoliday(new DateOnly(year, month, day)));
    }

    [Fact]
    public void IsHoliday_ShouldNotTreatBlackConsciousnessAsHolidayBefore2024()
    {
        Assert.False(BrazilianNationalHolidays.IsHoliday(new DateOnly(2023, 11, 20)));
    }

    [Fact]
    public void IsHoliday_ShouldReturnFalseForRegularWeekday()
    {
        Assert.False(BrazilianNationalHolidays.IsHoliday(new DateOnly(2026, 3, 10)));
    }

    [Fact]
    public void GetHolidays_ShouldRejectInvertedYearRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => BrazilianNationalHolidays.GetHolidays(2026, 2025));
    }

    [Fact]
    public void GetHolidays_ShouldIncludeYearsInRange()
    {
        var holidays = BrazilianNationalHolidays.GetHolidays(2026, 2027);

        Assert.Contains(new DateOnly(2026, 1, 1), holidays);
        Assert.Contains(new DateOnly(2027, 1, 1), holidays);
    }
}

public class FinancialCalendarTests
{
    private readonly FinancialCalendar _calendar = new();

    [Fact]
    public void BusinessDaysPerYear_ShouldBe252()
    {
        Assert.Equal(252, FinancialCalendar.BusinessDaysPerYear);
    }

    [Theory]
    [InlineData(2026, 7, 11)] // Saturday
    [InlineData(2026, 7, 12)] // Sunday
    public void IsWeekend_ShouldDetectWeekends(int year, int month, int day)
    {
        Assert.True(FinancialCalendar.IsWeekend(new DateOnly(year, month, day)));
    }

    [Fact]
    public void IsWeekend_ShouldReturnFalseForWeekday()
    {
        Assert.False(FinancialCalendar.IsWeekend(new DateOnly(2026, 7, 10))); // Friday
    }

    [Fact]
    public void IsBusinessDay_ShouldReturnFalseForWeekend()
    {
        Assert.False(_calendar.IsBusinessDay(new DateOnly(2026, 7, 11)));
    }

    [Fact]
    public void IsBusinessDay_ShouldReturnFalseForHoliday()
    {
        Assert.False(_calendar.IsBusinessDay(new DateOnly(2026, 1, 1)));
    }

    [Fact]
    public void IsBusinessDay_ShouldReturnTrueForRegularWeekday()
    {
        Assert.True(_calendar.IsBusinessDay(new DateOnly(2026, 7, 10)));
    }

    [Fact]
    public void GetNextBusinessDay_ShouldSkipWeekend()
    {
        // Friday → next is Monday
        var next = _calendar.GetNextBusinessDay(new DateOnly(2026, 7, 10));

        Assert.Equal(new DateOnly(2026, 7, 13), next);
    }

    [Fact]
    public void GetNextBusinessDay_ShouldSkipHoliday()
    {
        // 2026-12-24 (Thu) → next business day skips Christmas (Fri) → Monday 28
        var next = _calendar.GetNextBusinessDay(new DateOnly(2026, 12, 24));

        Assert.Equal(new DateOnly(2026, 12, 28), next);
    }

    [Fact]
    public void GetSameOrNextBusinessDay_ShouldKeepBusinessDay()
    {
        var date = new DateOnly(2026, 7, 10);

        Assert.Equal(date, _calendar.GetSameOrNextBusinessDay(date));
    }

    [Fact]
    public void AdjustContributionDate_Postpone_ShouldMoveWeekendToMonday()
    {
        var calendar = new FinancialCalendar(NonBusinessDayContributionRule.PostponeToNextBusinessDay);

        var adjusted = calendar.AdjustContributionDate(new DateOnly(2026, 7, 11)); // Saturday

        Assert.Equal(new DateOnly(2026, 7, 13), adjusted);
    }

    [Fact]
    public void AdjustContributionDate_Postpone_ShouldMoveHolidayToNextBusinessDay()
    {
        var calendar = new FinancialCalendar(NonBusinessDayContributionRule.PostponeToNextBusinessDay);

        // New Year 2026 is Thursday → next business day is Friday 2
        var adjusted = calendar.AdjustContributionDate(new DateOnly(2026, 1, 1));

        Assert.Equal(new DateOnly(2026, 1, 2), adjusted);
    }

    [Fact]
    public void AdjustContributionDate_KeepOriginal_ShouldPreserveNonBusinessDay()
    {
        var calendar = new FinancialCalendar(NonBusinessDayContributionRule.KeepOriginalDate);
        var saturday = new DateOnly(2026, 7, 11);

        Assert.Equal(saturday, calendar.AdjustContributionDate(saturday));
    }

    [Fact]
    public void Constructor_ShouldRejectUndefinedContributionRule()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new FinancialCalendar((NonBusinessDayContributionRule)999));
    }

    [Fact]
    public void CountCalendarDays_ShouldReturnElapsedDays()
    {
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 1, 31);

        Assert.Equal(30, FinancialCalendar.CountCalendarDays(start, end));
    }

    [Fact]
    public void CountCalendarDays_ShouldReturnZeroForSameDate()
    {
        var date = new DateOnly(2026, 1, 1);

        Assert.Equal(0, FinancialCalendar.CountCalendarDays(date, date));
    }

    [Fact]
    public void CountCalendarDays_ShouldRejectInvertedRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => FinancialCalendar.CountCalendarDays(
                new DateOnly(2026, 2, 1),
                new DateOnly(2026, 1, 1)));
    }

    [Fact]
    public void CountBusinessDays_ShouldExcludeWeekendsAndHolidays()
    {
        // 2026-01-01 (Thu, holiday) → 2026-01-09 (Fri)
        // Business days in (start, end]: 2,5,6,7,8,9 = 6 (skips 1 holiday, 3-4 weekend)
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 1, 9);

        Assert.Equal(6, _calendar.CountBusinessDays(start, end));
    }

    [Fact]
    public void CountBusinessDays_ShouldReturnZeroForSameDate()
    {
        var date = new DateOnly(2026, 7, 10);

        Assert.Equal(0, _calendar.CountBusinessDays(date, date));
    }

    [Fact]
    public void CountBusinessDays_ShouldRejectInvertedRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => _calendar.CountBusinessDays(
                new DateOnly(2026, 2, 1),
                new DateOnly(2026, 1, 1)));
    }

    [Fact]
    public void EnumerateBusinessDays_ShouldMatchCountBusinessDays()
    {
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 1, 31);

        var enumerated = _calendar.EnumerateBusinessDays(start, end).ToList();

        Assert.Equal(_calendar.CountBusinessDays(start, end), enumerated.Count);
        Assert.All(enumerated, day => Assert.True(_calendar.IsBusinessDay(day)));
    }

    [Fact]
    public void CustomHolidayPredicate_ShouldBeHonoured()
    {
        var customHoliday = new DateOnly(2026, 7, 10); // Friday
        var calendar = new FinancialCalendar(isHoliday: date => date == customHoliday);

        Assert.False(calendar.IsBusinessDay(customHoliday));
        Assert.Equal(new DateOnly(2026, 7, 13), calendar.GetNextBusinessDay(new DateOnly(2026, 7, 9)));
    }

    [Fact]
    public void CalendarDaysVersusBusinessDays_ShouldDifferOverWeekend()
    {
        var start = new DateOnly(2026, 7, 10); // Friday
        var end = new DateOnly(2026, 7, 13);   // Monday

        // IR/IOF: 3 calendar days; profitability: 1 business day (Monday only in (Fri, Mon])
        Assert.Equal(3, FinancialCalendar.CountCalendarDays(start, end));
        Assert.Equal(1, _calendar.CountBusinessDays(start, end));
    }
}
