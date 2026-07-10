using InvestmentSimulator.Domain.Entities;
using InvestmentSimulator.Domain.Enums;
using InvestmentSimulator.Infrastructure.Persistence;

namespace InvestmentSimulator.Infrastructure.Tests.Persistence;

public class InMemorySimulationHistoryRepositoryTests
{
    private readonly InMemorySimulationHistoryRepository _sut = new();

    [Fact]
    public void Save_ThenGetById_ShouldRoundTripEntry()
    {
        var entry = CreateEntry(
            name: "CDB 110%",
            date: new DateOnly(2026, 7, 10),
            type: InvestmentType.Cdb,
            observations: "Primeiro cenário");

        var saved = _sut.Save(entry);
        var loaded = _sut.GetById(saved.Id);

        Assert.Same(entry, saved);
        Assert.NotNull(loaded);
        Assert.Equal(entry.Id, loaded.Id);
        Assert.Equal("CDB 110%", loaded.Name);
        Assert.Equal(new DateOnly(2026, 7, 10), loaded.Date);
        Assert.Equal(InvestmentType.Cdb, loaded.Type);
        Assert.Equal("Primeiro cenário", loaded.Observations);
        Assert.Same(entry.Simulation, loaded.Simulation);
    }

    [Fact]
    public void GetById_WhenMissing_ShouldReturnNull()
    {
        var loaded = _sut.GetById(Guid.NewGuid());

        Assert.Null(loaded);
    }

    [Fact]
    public void List_ShouldReturnEntriesOrderedByDateDescendingThenName()
    {
        var older = CreateEntry("Zeta", new DateOnly(2026, 1, 1), InvestmentType.Cdb, "a");
        var newerB = CreateEntry("Beta", new DateOnly(2026, 7, 10), InvestmentType.TesouroSelic, "b");
        var newerA = CreateEntry("Alpha", new DateOnly(2026, 7, 10), InvestmentType.Cdb, "c");

        _sut.Save(older);
        _sut.Save(newerB);
        _sut.Save(newerA);

        var list = _sut.List();

        Assert.Equal(3, list.Count);
        Assert.Equal(newerA.Id, list[0].Id);
        Assert.Equal(newerB.Id, list[1].Id);
        Assert.Equal(older.Id, list[2].Id);
    }

    [Fact]
    public void Save_WithSameId_ShouldOverwriteExistingEntry()
    {
        var id = Guid.NewGuid();
        var first = CreateEntry("Original", new DateOnly(2026, 3, 1), InvestmentType.Cdb, "v1", id);
        var updated = CreateEntry("Atualizado", new DateOnly(2026, 3, 2), InvestmentType.TesouroSelic, "v2", id);

        _sut.Save(first);
        _sut.Save(updated);

        var loaded = _sut.GetById(id);
        var list = _sut.List();

        Assert.NotNull(loaded);
        Assert.Equal("Atualizado", loaded.Name);
        Assert.Equal(new DateOnly(2026, 3, 2), loaded.Date);
        Assert.Equal(InvestmentType.TesouroSelic, loaded.Type);
        Assert.Equal("v2", loaded.Observations);
        Assert.Single(list);
    }

    [Fact]
    public void Save_ShouldRejectNullEntry()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.Save(null!));
    }

    private static SimulationHistoryEntry CreateEntry(
        string name,
        DateOnly date,
        InvestmentType type,
        string observations,
        Guid? id = null)
    {
        var simulation = new Simulation(
            type: type,
            initialAmount: 5_000m,
            initialContributionDate: new DateOnly(2026, 1, 1),
            endDate: new DateOnly(2026, 12, 31),
            contributions: [new Contribution(new DateOnly(2026, 6, 1), 500m)],
            annualRates: [new AnnualRate(2026, 0.14m)],
            ipcaRates: [new AnnualRate(2026, 0.04m)],
            profitabilityPercentage: 1.0m,
            costs: 0m);

        return new SimulationHistoryEntry(name, date, observations, simulation, id);
    }
}
