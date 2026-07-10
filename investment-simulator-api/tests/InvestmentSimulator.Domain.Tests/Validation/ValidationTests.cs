using InvestmentSimulator.Domain.Entities;
using InvestmentSimulator.Domain.Enums;
using InvestmentSimulator.Domain.Exceptions;

namespace InvestmentSimulator.Domain.Tests.Entities;

public class ContributionValidationTests
{
    [Fact]
    public void Constructor_ShouldRejectDefaultDate()
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => new Contribution(default, 1_000m));

        Assert.Contains("date", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void Constructor_ShouldRejectAmountLessThanOrEqualToZero(decimal amount)
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => new Contribution(new DateOnly(2026, 1, 1), amount));

        Assert.Contains("greater than zero", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_ShouldAcceptPositiveAmount()
    {
        var contribution = new Contribution(new DateOnly(2026, 1, 1), 0.01m);

        Assert.Equal(0.01m, contribution.Amount);
    }
}

public class AnnualRateValidationTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-2026)]
    public void Constructor_ShouldRejectInvalidYear(int year)
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => new AnnualRate(year, 0.15m));

        Assert.Contains("year", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_ShouldRejectNegativeRate()
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => new AnnualRate(2026, -0.01m));

        Assert.Contains("negative", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_ShouldAcceptZeroRate()
    {
        var annualRate = new AnnualRate(2026, 0m);

        Assert.Equal(0m, annualRate.Rate);
    }
}

public class SimulationValidationTests
{
    private static Simulation CreateValidSimulation(
        decimal initialAmount = 10_000m,
        DateOnly? initialContributionDate = null,
        DateOnly? endDate = null,
        IReadOnlyList<Contribution>? contributions = null,
        IReadOnlyList<AnnualRate>? annualRates = null,
        IReadOnlyList<AnnualRate>? ipcaRates = null,
        decimal profitabilityPercentage = 1.10m,
        decimal costs = 0m,
        InvestmentType type = InvestmentType.Cdb)
    {
        return new Simulation(
            type: type,
            initialAmount: initialAmount,
            initialContributionDate: initialContributionDate ?? new DateOnly(2026, 1, 1),
            endDate: endDate ?? new DateOnly(2027, 12, 31),
            contributions: contributions ?? [],
            annualRates: annualRates ?? [new AnnualRate(2026, 0.15m)],
            ipcaRates: ipcaRates ?? [new AnnualRate(2026, 0.05m)],
            profitabilityPercentage: profitabilityPercentage,
            costs: costs);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10_000)]
    public void Constructor_ShouldRejectInitialAmountLessThanOrEqualToZero(decimal initialAmount)
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => CreateValidSimulation(initialAmount: initialAmount));

        Assert.Contains("Initial amount", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_ShouldRejectDefaultInitialContributionDate()
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => new Simulation(
                type: InvestmentType.Cdb,
                initialAmount: 10_000m,
                initialContributionDate: default,
                endDate: new DateOnly(2027, 12, 31),
                contributions: [],
                annualRates: [new AnnualRate(2026, 0.15m)],
                ipcaRates: [new AnnualRate(2026, 0.05m)],
                profitabilityPercentage: 1.10m,
                costs: 0m));

        Assert.Contains("Initial contribution date", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_ShouldRejectDefaultEndDate()
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => new Simulation(
                type: InvestmentType.Cdb,
                initialAmount: 10_000m,
                initialContributionDate: new DateOnly(2026, 1, 1),
                endDate: default,
                contributions: [],
                annualRates: [new AnnualRate(2026, 0.15m)],
                ipcaRates: [new AnnualRate(2026, 0.05m)],
                profitabilityPercentage: 1.10m,
                costs: 0m));

        Assert.Contains("End (redemption) date", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_ShouldRejectRedemptionBeforeInitialContribution()
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => CreateValidSimulation(
                initialContributionDate: new DateOnly(2027, 1, 1),
                endDate: new DateOnly(2026, 1, 1)));

        Assert.Contains("Redemption date", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_ShouldAcceptRedemptionOnSameDayAsInitialContribution()
    {
        var simulation = CreateValidSimulation(
            initialContributionDate: new DateOnly(2026, 6, 15),
            endDate: new DateOnly(2026, 6, 15));

        Assert.Equal(simulation.InitialContributionDate, simulation.EndDate);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-1.10)]
    public void Constructor_ShouldRejectInvalidProfitabilityPercentage(decimal profitabilityPercentage)
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => CreateValidSimulation(profitabilityPercentage: profitabilityPercentage));

        Assert.Contains("Profitability percentage", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_ShouldRejectNegativeCosts()
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => CreateValidSimulation(costs: -1m));

        Assert.Contains("Costs", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_ShouldRejectNullContributions()
    {
        Assert.Throws<ArgumentNullException>(
            () => new Simulation(
                type: InvestmentType.Cdb,
                initialAmount: 10_000m,
                initialContributionDate: new DateOnly(2026, 1, 1),
                endDate: new DateOnly(2027, 12, 31),
                contributions: null!,
                annualRates: [new AnnualRate(2026, 0.15m)],
                ipcaRates: [new AnnualRate(2026, 0.05m)],
                profitabilityPercentage: 1.10m,
                costs: 0m));
    }

    [Fact]
    public void Constructor_ShouldRejectNullAnnualRates()
    {
        Assert.Throws<ArgumentNullException>(
            () => new Simulation(
                type: InvestmentType.Cdb,
                initialAmount: 10_000m,
                initialContributionDate: new DateOnly(2026, 1, 1),
                endDate: new DateOnly(2027, 12, 31),
                contributions: [],
                annualRates: null!,
                ipcaRates: [new AnnualRate(2026, 0.05m)],
                profitabilityPercentage: 1.10m,
                costs: 0m));
    }

    [Fact]
    public void Constructor_ShouldRejectNullIpcaRates()
    {
        Assert.Throws<ArgumentNullException>(
            () => new Simulation(
                type: InvestmentType.Cdb,
                initialAmount: 10_000m,
                initialContributionDate: new DateOnly(2026, 1, 1),
                endDate: new DateOnly(2027, 12, 31),
                contributions: [],
                annualRates: [new AnnualRate(2026, 0.15m)],
                ipcaRates: null!,
                profitabilityPercentage: 1.10m,
                costs: 0m));
    }

    [Fact]
    public void Constructor_ShouldRejectContributionBeforeInitialDate()
    {
        var contributions = new List<Contribution>
        {
            new(new DateOnly(2025, 12, 31), 500m),
        };

        var exception = Assert.Throws<DomainValidationException>(
            () => CreateValidSimulation(contributions: contributions));

        Assert.Contains("earlier than the initial contribution date", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_ShouldRejectContributionAfterRedemptionDate()
    {
        var contributions = new List<Contribution>
        {
            new(new DateOnly(2028, 1, 1), 500m),
        };

        var exception = Assert.Throws<DomainValidationException>(
            () => CreateValidSimulation(contributions: contributions));

        Assert.Contains("after the redemption date", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_ShouldRejectContributionsOutOfChronologicalOrder()
    {
        var contributions = new List<Contribution>
        {
            new(new DateOnly(2026, 3, 1), 1_200m),
            new(new DateOnly(2026, 2, 1), 900m),
        };

        var exception = Assert.Throws<DomainValidationException>(
            () => CreateValidSimulation(contributions: contributions));

        Assert.Contains("chronological order", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_ShouldAcceptChronologicalContributionsWithinPeriod()
    {
        var contributions = new List<Contribution>
        {
            new(new DateOnly(2026, 1, 1), 100m),
            new(new DateOnly(2026, 2, 1), 900m),
            new(new DateOnly(2026, 3, 1), 1_200m),
            new(new DateOnly(2027, 12, 31), 500m),
        };

        var simulation = CreateValidSimulation(contributions: contributions);

        Assert.Equal(4, simulation.Contributions.Count);
    }

    [Fact]
    public void Constructor_ShouldRejectNullContributionEntry()
    {
        var contributions = new Contribution[]
        {
            new(new DateOnly(2026, 2, 1), 900m),
            null!,
        };

        var exception = Assert.Throws<DomainValidationException>(
            () => CreateValidSimulation(contributions: contributions));

        Assert.Contains("required", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_ShouldRejectNullAnnualRateEntry()
    {
        var annualRates = new AnnualRate[]
        {
            new(2026, 0.15m),
            null!,
        };

        var exception = Assert.Throws<DomainValidationException>(
            () => CreateValidSimulation(annualRates: annualRates));

        Assert.Contains("annualRates", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_ShouldRejectInvalidInvestmentType()
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => CreateValidSimulation(type: (InvestmentType)999));

        Assert.Contains("Investment type", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
