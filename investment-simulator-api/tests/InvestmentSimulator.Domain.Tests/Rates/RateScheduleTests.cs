using InvestmentSimulator.Domain.Calendar;
using InvestmentSimulator.Domain.Common;
using InvestmentSimulator.Domain.Entities;
using InvestmentSimulator.Domain.Exceptions;
using InvestmentSimulator.Domain.Rates;

namespace InvestmentSimulator.Domain.Tests.Rates;

public class RateConverterTests
{
    [Fact]
    public void AnnualToDaily_ShouldApplyErsSection8Formula()
    {
        const decimal annualRate = 0.15m;

        var dailyRate = RateConverter.AnnualToDaily(annualRate);

        var expected = (decimal)Math.Pow(1.15, 1.0 / FinancialCalendar.BusinessDaysPerYear) - 1m;
        expected = Math.Round(expected, MonetaryPrecision.IntermediateDecimalPlaces, MidpointRounding.AwayFromZero);

        Assert.Equal(expected, dailyRate);
    }

    [Fact]
    public void AnnualToDaily_ShouldReturnZero_WhenAnnualRateIsZero()
    {
        Assert.Equal(0m, RateConverter.AnnualToDaily(0m));
    }

    [Fact]
    public void AnnualToDaily_ShouldRoundToIntermediatePrecision()
    {
        var dailyRate = RateConverter.AnnualToDaily(0.15m);
        var scaled = dailyRate * (decimal)Math.Pow(10, MonetaryPrecision.IntermediateDecimalPlaces);

        Assert.Equal(scaled, Math.Truncate(scaled));
    }

    [Fact]
    public void AnnualToDaily_ShouldCompoundBackCloseToAnnualRate()
    {
        const decimal annualRate = 0.15m;
        var dailyRate = RateConverter.AnnualToDaily(annualRate);

        var compounded = 1m;
        for (var i = 0; i < FinancialCalendar.BusinessDaysPerYear; i++)
        {
            compounded *= 1m + dailyRate;
        }

        var impliedAnnual = compounded - 1m;

        Assert.Equal(annualRate, impliedAnnual, precision: 4);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-1.5)]
    public void AnnualToDaily_ShouldRejectRateLessThanOrEqualToMinusOne(decimal annualRate)
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => RateConverter.AnnualToDaily(annualRate));

        Assert.Contains("-1", exception.Message, StringComparison.Ordinal);
    }
}

public class YearGeneratorTests
{
    [Fact]
    public void Generate_ShouldReturnInclusiveYears_MatchingErsSection7Example()
    {
        var start = new DateOnly(2026, 8, 10);
        var end = new DateOnly(2031, 4, 15);

        var years = YearGenerator.Generate(start, end);

        Assert.Equal([2026, 2027, 2028, 2029, 2030, 2031], years);
    }

    [Fact]
    public void Generate_ShouldReturnSingleYear_WhenDatesAreInSameYear()
    {
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 12, 31);

        var years = YearGenerator.Generate(start, end);

        Assert.Equal([2026], years);
    }

    [Fact]
    public void Generate_ShouldRejectEndBeforeStart()
    {
        var start = new DateOnly(2030, 1, 1);
        var end = new DateOnly(2026, 1, 1);

        Assert.Throws<ArgumentOutOfRangeException>(() => YearGenerator.Generate(start, end));
    }
}

public class RateScheduleTests
{
    private static readonly DateOnly Start = new(2026, 8, 10);
    private static readonly DateOnly End = new(2031, 4, 15);

    [Fact]
    public void FromSingleRate_ShouldExpandRateAcrossAllYears()
    {
        const decimal rate = 0.15m;

        var schedule = RateSchedule.FromSingleRate(rate, Start, End);

        Assert.Equal(RateEntryMode.SingleRate, schedule.Mode);
        Assert.Equal([2026, 2027, 2028, 2029, 2030, 2031], schedule.Rates.Select(r => r.Year));
        Assert.All(schedule.Rates, r => Assert.Equal(rate, r.Rate));
        Assert.Equal(rate, schedule.GetAnnualRate(2028));
        Assert.Equal(RateConverter.AnnualToDaily(rate), schedule.GetDailyRate(2028));
    }

    [Fact]
    public void FromPerYear_ShouldAcceptDistinctRatePerYear()
    {
        var rates = new List<AnnualRate>
        {
            new(2026, 0.15m),
            new(2027, 0.13m),
            new(2028, 0.11m),
            new(2029, 0.10m),
            new(2030, 0.09m),
            new(2031, 0.08m),
        };

        var schedule = RateSchedule.FromPerYear(rates, Start, End);

        Assert.Equal(RateEntryMode.PerYear, schedule.Mode);
        Assert.Equal(0.13m, schedule.GetAnnualRate(2027));
        Assert.Equal(RateConverter.AnnualToDaily(0.11m), schedule.GetDailyRate(2028));
    }

    [Fact]
    public void FromPerYear_ShouldOrderRatesByYear_RegardlessOfInputOrder()
    {
        var rates = new List<AnnualRate>
        {
            new(2031, 0.08m),
            new(2026, 0.15m),
            new(2028, 0.11m),
            new(2027, 0.13m),
            new(2030, 0.09m),
            new(2029, 0.10m),
        };

        var schedule = RateSchedule.FromPerYear(rates, Start, End);

        Assert.Equal([2026, 2027, 2028, 2029, 2030, 2031], schedule.Rates.Select(r => r.Year));
    }

    [Fact]
    public void FromPerYear_ShouldRejectWrongCount()
    {
        var rates = new List<AnnualRate>
        {
            new(2026, 0.15m),
            new(2027, 0.13m),
        };

        var exception = Assert.Throws<DomainValidationException>(
            () => RateSchedule.FromPerYear(rates, Start, End));

        Assert.Contains("exactly", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FromPerYear_ShouldRejectDuplicateYear()
    {
        var rates = new List<AnnualRate>
        {
            new(2026, 0.15m),
            new(2026, 0.14m),
            new(2028, 0.11m),
            new(2029, 0.10m),
            new(2030, 0.09m),
            new(2031, 0.08m),
        };

        var exception = Assert.Throws<DomainValidationException>(
            () => RateSchedule.FromPerYear(rates, Start, End));

        Assert.Contains("Duplicate", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FromPerYear_ShouldRejectMissingYear()
    {
        var rates = new List<AnnualRate>
        {
            new(2026, 0.15m),
            new(2027, 0.13m),
            new(2028, 0.11m),
            new(2029, 0.10m),
            new(2030, 0.09m),
            new(2032, 0.08m), // wrong year instead of 2031
        };

        var exception = Assert.Throws<DomainValidationException>(
            () => RateSchedule.FromPerYear(rates, Start, End));

        Assert.Contains("2031", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void FromPerYear_ShouldRejectNullList()
    {
        Assert.Throws<ArgumentNullException>(
            () => RateSchedule.FromPerYear(null!, Start, End));
    }

    [Fact]
    public void GetAnnualRate_ShouldRejectUnknownYear()
    {
        var schedule = RateSchedule.FromSingleRate(0.15m, Start, End);

        var exception = Assert.Throws<DomainValidationException>(
            () => schedule.GetAnnualRate(2040));

        Assert.Contains("2040", exception.Message, StringComparison.Ordinal);
    }
}
