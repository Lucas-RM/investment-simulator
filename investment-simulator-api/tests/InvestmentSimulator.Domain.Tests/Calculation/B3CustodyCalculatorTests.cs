using InvestmentSimulator.Domain.Calculation;
using InvestmentSimulator.Domain.Calendar;
using InvestmentSimulator.Domain.Common;
using InvestmentSimulator.Domain.Exceptions;
using InvestmentSimulator.Domain.Rates;

namespace InvestmentSimulator.Domain.Tests.Calculation;

public class B3CustodyCalculatorTests
{
    private static readonly FinancialCalendar Calendar = new(isHoliday: _ => false);

    [Theory]
    [InlineData(0, 0)]
    [InlineData(5_000, 0)]
    [InlineData(10_000, 0)]
    [InlineData(10_000.01, 0.01)]
    [InlineData(15_000, 5_000)]
    [InlineData(50_000, 40_000)]
    public void CalculateTaxableBase_ShouldExemptUpToTenThousand(decimal balance, decimal expected)
    {
        var result = B3CustodyCalculator.CalculateTaxableBase(balance);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateTaxableBase_ShouldRejectNegativeBalance()
    {
        Assert.Throws<DomainValidationException>(() =>
            B3CustodyCalculator.CalculateTaxableBase(-1m));
    }

    [Fact]
    public void CalculateDailyProvision_WhenBalanceAtOrBelowThreshold_ShouldBeZero()
    {
        var dailyRate = RateConverter.AnnualToDaily(0.002m);

        Assert.Equal(0m, B3CustodyCalculator.CalculateDailyProvision(10_000m, dailyRate));
        Assert.Equal(0m, B3CustodyCalculator.CalculateDailyProvision(5_000m, dailyRate));
    }

    [Fact]
    public void CalculateDailyProvision_ShouldChargeOnlyOnExcess()
    {
        var dailyRate = RateConverter.AnnualToDaily(0.002m);
        const decimal balance = 15_000m;
        const decimal excess = 5_000m;

        var result = B3CustodyCalculator.CalculateDailyProvision(balance, dailyRate);

        var expected = Math.Round(
            excess * dailyRate,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);

        Assert.Equal(expected, result);
        Assert.True(result > 0m);
    }

    [Fact]
    public void CalculateDailyProvision_WithZeroRate_ShouldBeZero()
    {
        Assert.Equal(0m, B3CustodyCalculator.CalculateDailyProvision(50_000m, 0m));
    }

    [Fact]
    public void CalculateDailyProvision_ShouldRejectNegativeRate()
    {
        Assert.Throws<DomainValidationException>(() =>
            B3CustodyCalculator.CalculateDailyProvision(15_000m, -0.0001m));
    }

    [Fact]
    public void GetSemiannualCollectionDate_January_ShouldBeFirstBusinessDay()
    {
        // 2026-01-01 is Thursday
        var date = B3CustodyCalculator.GetSemiannualCollectionDate(2026, 1, Calendar);

        Assert.Equal(new DateOnly(2026, 1, 1), date);
    }

    [Fact]
    public void GetSemiannualCollectionDate_JulyOnWeekend_ShouldSkipToMonday()
    {
        // 2027-07-01 is Thursday — use a year where July 1 is Saturday: 2028-07-01 is Saturday
        var date = B3CustodyCalculator.GetSemiannualCollectionDate(2028, 7, Calendar);

        Assert.Equal(new DateOnly(2028, 7, 3), date); // Monday
    }

    [Fact]
    public void GetSemiannualCollectionDate_ShouldRejectInvalidMonth()
    {
        Assert.Throws<DomainValidationException>(() =>
            B3CustodyCalculator.GetSemiannualCollectionDate(2026, 6, Calendar));
    }

    [Fact]
    public void GetSemiannualCollectionDate_ShouldRejectNullCalendar()
    {
        Assert.Throws<ArgumentNullException>(() =>
            B3CustodyCalculator.GetSemiannualCollectionDate(2026, 1, null!));
    }

    [Fact]
    public void IsSemiannualCollectionDate_ShouldRecognizeJanuaryAndJulyFirstBusinessDays()
    {
        Assert.True(B3CustodyCalculator.IsSemiannualCollectionDate(new DateOnly(2026, 1, 1), Calendar));
        Assert.True(B3CustodyCalculator.IsSemiannualCollectionDate(new DateOnly(2026, 7, 1), Calendar));
        Assert.False(B3CustodyCalculator.IsSemiannualCollectionDate(new DateOnly(2026, 1, 2), Calendar));
        Assert.False(B3CustodyCalculator.IsSemiannualCollectionDate(new DateOnly(2026, 6, 1), Calendar));
    }

    [Fact]
    public void IsSemiannualCollectionDate_WhenFirstOfMonthIsWeekend_ShouldUseNextBusinessDay()
    {
        // 2028-07-01 Saturday → collection on 2028-07-03
        Assert.False(B3CustodyCalculator.IsSemiannualCollectionDate(new DateOnly(2028, 7, 1), Calendar));
        Assert.True(B3CustodyCalculator.IsSemiannualCollectionDate(new DateOnly(2028, 7, 3), Calendar));
    }
}

public class B3CustodyProvisionerTests
{
    private static readonly FinancialCalendar Calendar = new(isHoliday: _ => false);

    [Fact]
    public void Constructor_ShouldRejectNullCalendar()
    {
        Assert.Throws<ArgumentNullException>(() => new B3CustodyProvisioner(null!));
    }

    [Fact]
    public void AccrueDaily_WhenExempt_ShouldNotIncreaseProvision()
    {
        var provisioner = new B3CustodyProvisioner(Calendar);
        var dailyRate = RateConverter.AnnualToDaily(0.002m);

        provisioner.AccrueDaily(10_000m, dailyRate);

        Assert.Equal(0m, provisioner.ProvisionedAmount);
        Assert.Equal(0m, provisioner.TotalCollected);
    }

    [Fact]
    public void AccrueDaily_ShouldAccumulateDailyProvisions()
    {
        var provisioner = new B3CustodyProvisioner(Calendar);
        var dailyRate = RateConverter.AnnualToDaily(0.002m);
        var dailyProvision = B3CustodyCalculator.CalculateDailyProvision(15_000m, dailyRate);

        provisioner.AccrueDaily(15_000m, dailyRate);
        provisioner.AccrueDaily(15_000m, dailyRate);

        var expected = Math.Round(
            dailyProvision * 2m,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);

        Assert.Equal(expected, provisioner.ProvisionedAmount);
        Assert.Equal(0m, provisioner.TotalCollected);
    }

    [Fact]
    public void ProcessBusinessDay_OnNonCollectionDate_ShouldOnlyAccrue()
    {
        var provisioner = new B3CustodyProvisioner(Calendar);
        var dailyRate = RateConverter.AnnualToDaily(0.002m);
        var day = new DateOnly(2026, 3, 10); // not Jan/Jul collection

        var collected = provisioner.ProcessBusinessDay(day, 20_000m, dailyRate);

        Assert.Equal(0m, collected);
        Assert.True(provisioner.ProvisionedAmount > 0m);
        Assert.Equal(0m, provisioner.TotalCollected);
    }

    [Fact]
    public void ProcessBusinessDay_OnSemiannualCollectionDate_ShouldAccrueAndCollect()
    {
        var provisioner = new B3CustodyProvisioner(Calendar);
        var dailyRate = RateConverter.AnnualToDaily(0.002m);

        // Accrue before collection date
        provisioner.AccrueDaily(20_000m, dailyRate);
        provisioner.AccrueDaily(20_000m, dailyRate);
        var provisionedBefore = provisioner.ProvisionedAmount;

        var collectionDay = new DateOnly(2026, 7, 1);
        var collected = provisioner.ProcessBusinessDay(collectionDay, 20_000m, dailyRate);

        var dayOfCollectionProvision = B3CustodyCalculator.CalculateDailyProvision(20_000m, dailyRate);
        var expectedCollected = Math.Round(
            provisionedBefore + dayOfCollectionProvision,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);

        Assert.Equal(expectedCollected, collected);
        Assert.Equal(0m, provisioner.ProvisionedAmount);
        Assert.Equal(expectedCollected, provisioner.TotalCollected);
    }

    [Fact]
    public void CollectOnRedemption_ShouldChargeRemainingProvision()
    {
        var provisioner = new B3CustodyProvisioner(Calendar);
        var dailyRate = RateConverter.AnnualToDaily(0.002m);

        provisioner.AccrueDaily(25_000m, dailyRate);
        provisioner.AccrueDaily(25_000m, dailyRate);
        var expected = provisioner.ProvisionedAmount;

        var collected = provisioner.CollectOnRedemption();

        Assert.Equal(expected, collected);
        Assert.Equal(0m, provisioner.ProvisionedAmount);
        Assert.Equal(expected, provisioner.TotalCollected);
    }

    [Fact]
    public void CollectOnRedemption_WhenNothingProvisioned_ShouldReturnZero()
    {
        var provisioner = new B3CustodyProvisioner(Calendar);

        Assert.Equal(0m, provisioner.CollectOnRedemption());
        Assert.Equal(0m, provisioner.TotalCollected);
    }

    [Fact]
    public void MultipleCollections_ShouldAccumulateTotalCollected()
    {
        var provisioner = new B3CustodyProvisioner(Calendar);
        var dailyRate = RateConverter.AnnualToDaily(0.002m);

        provisioner.ProcessBusinessDay(new DateOnly(2026, 1, 1), 30_000m, dailyRate);
        var afterJanuary = provisioner.TotalCollected;

        provisioner.AccrueDaily(30_000m, dailyRate);
        provisioner.ProcessBusinessDay(new DateOnly(2026, 7, 1), 30_000m, dailyRate);

        Assert.True(afterJanuary > 0m);
        Assert.True(provisioner.TotalCollected > afterJanuary);
        Assert.Equal(0m, provisioner.ProvisionedAmount);
    }

    [Fact]
    public void ProcessBusinessDay_ShouldRejectDefaultDate()
    {
        var provisioner = new B3CustodyProvisioner(Calendar);

        Assert.Throws<DomainValidationException>(() =>
            provisioner.ProcessBusinessDay(default, 15_000m, 0.0001m));
    }
}
