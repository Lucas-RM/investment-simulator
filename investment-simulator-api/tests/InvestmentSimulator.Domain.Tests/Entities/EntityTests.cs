using InvestmentSimulator.Domain.Entities;
using InvestmentSimulator.Domain.Enums;

namespace InvestmentSimulator.Domain.Tests.Entities;

public class ContributionTests
{
    [Fact]
    public void Constructor_ShouldAssignDateAndAmount()
    {
        var date = new DateOnly(2026, 1, 1);
        const decimal amount = 10_000m;

        var contribution = new Contribution(date, amount);

        Assert.Equal(date, contribution.Date);
        Assert.Equal(amount, contribution.Amount);
    }
}

public class AnnualRateTests
{
    [Fact]
    public void Constructor_ShouldAssignYearAndRateAsDecimalFraction()
    {
        const int year = 2026;
        const decimal rate = 0.15m;

        var annualRate = new AnnualRate(year, rate);

        Assert.Equal(year, annualRate.Year);
        Assert.Equal(rate, annualRate.Rate);
    }
}

public class SimulationTests
{
    [Fact]
    public void Constructor_ShouldAssignGeneralInputsFromErsSection3()
    {
        var contributions = new List<Contribution>
        {
            new(new DateOnly(2026, 2, 1), 900m),
            new(new DateOnly(2026, 3, 1), 1_200m),
        };
        var annualRates = new List<AnnualRate>
        {
            new(2026, 0.15m),
            new(2027, 0.13m),
        };
        var ipcaRates = new List<AnnualRate>
        {
            new(2026, 0.05m),
        };

        var simulation = new Simulation(
            type: InvestmentType.Cdb,
            initialAmount: 10_000m,
            initialContributionDate: new DateOnly(2026, 1, 1),
            endDate: new DateOnly(2027, 12, 31),
            contributions: contributions,
            annualRates: annualRates,
            ipcaRates: ipcaRates,
            profitabilityPercentage: 1.10m,
            costs: 0m);

        Assert.Equal(InvestmentType.Cdb, simulation.Type);
        Assert.Equal(10_000m, simulation.InitialAmount);
        Assert.Equal(new DateOnly(2026, 1, 1), simulation.InitialContributionDate);
        Assert.Equal(new DateOnly(2027, 12, 31), simulation.EndDate);
        Assert.Equal(2, simulation.Contributions.Count);
        Assert.Equal(2, simulation.AnnualRates.Count);
        Assert.Single(simulation.IpcaRates);
        Assert.Equal(1.10m, simulation.ProfitabilityPercentage);
        Assert.Equal(0m, simulation.Costs);
    }

    [Fact]
    public void Constructor_ShouldSupportTesouroSelicType()
    {
        var simulation = new Simulation(
            type: InvestmentType.TesouroSelic,
            initialAmount: 5_000m,
            initialContributionDate: new DateOnly(2026, 8, 10),
            endDate: new DateOnly(2031, 4, 15),
            contributions: [],
            annualRates: [new AnnualRate(2026, 0.1475m)],
            ipcaRates: [new AnnualRate(2026, 0.045m)],
            profitabilityPercentage: 1.0m,
            costs: 0.00025m);

        Assert.Equal(InvestmentType.TesouroSelic, simulation.Type);
        Assert.Empty(simulation.Contributions);
        Assert.Equal(0.00025m, simulation.Costs);
    }
}
