using InvestmentSimulator.Domain.Entities;
using InvestmentSimulator.Domain.Enums;
using InvestmentSimulator.Domain.Exceptions;

namespace InvestmentSimulator.Domain.Tests.Entities;

public class SimulationHistoryEntryTests
{
    [Fact]
    public void Constructor_ShouldAssignNameDateTypeObservationsAndSimulation()
    {
        var simulation = CreateSimulation(InvestmentType.Cdb);
        var date = new DateOnly(2026, 7, 10);

        var entry = new SimulationHistoryEntry(
            name: "  Reserva de emergência  ",
            date: date,
            observations: "Cenário base com 110% CDI",
            simulation: simulation);

        Assert.NotEqual(Guid.Empty, entry.Id);
        Assert.Equal("Reserva de emergência", entry.Name);
        Assert.Equal(date, entry.Date);
        Assert.Equal(InvestmentType.Cdb, entry.Type);
        Assert.Equal("Cenário base com 110% CDI", entry.Observations);
        Assert.Same(simulation, entry.Simulation);
    }

    [Fact]
    public void Constructor_ShouldPreserveProvidedId()
    {
        var id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var simulation = CreateSimulation(InvestmentType.TesouroSelic);

        var entry = new SimulationHistoryEntry(
            name: "Tesouro longo prazo",
            date: new DateOnly(2026, 1, 15),
            observations: string.Empty,
            simulation: simulation,
            id: id);

        Assert.Equal(id, entry.Id);
        Assert.Equal(InvestmentType.TesouroSelic, entry.Type);
        Assert.Equal(string.Empty, entry.Observations);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldRejectMissingName(string? name)
    {
        var simulation = CreateSimulation(InvestmentType.Cdb);

        var exception = Assert.Throws<DomainValidationException>(() =>
            new SimulationHistoryEntry(
                name: name!,
                date: new DateOnly(2026, 7, 10),
                observations: "ok",
                simulation: simulation));

        Assert.Contains("name", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_ShouldRejectDefaultDate()
    {
        var simulation = CreateSimulation(InvestmentType.Cdb);

        var exception = Assert.Throws<DomainValidationException>(() =>
            new SimulationHistoryEntry(
                name: "Simulação",
                date: default,
                observations: "ok",
                simulation: simulation));

        Assert.Contains("date", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_ShouldRejectNullObservations()
    {
        var simulation = CreateSimulation(InvestmentType.Cdb);

        Assert.Throws<ArgumentNullException>(() =>
            new SimulationHistoryEntry(
                name: "Simulação",
                date: new DateOnly(2026, 7, 10),
                observations: null!,
                simulation: simulation));
    }

    [Fact]
    public void Constructor_ShouldRejectNullSimulation()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SimulationHistoryEntry(
                name: "Simulação",
                date: new DateOnly(2026, 7, 10),
                observations: "ok",
                simulation: null!));
    }

    private static Simulation CreateSimulation(InvestmentType type) =>
        new(
            type: type,
            initialAmount: 10_000m,
            initialContributionDate: new DateOnly(2026, 1, 1),
            endDate: new DateOnly(2027, 12, 31),
            contributions: [],
            annualRates: [new AnnualRate(2026, 0.15m)],
            ipcaRates: [new AnnualRate(2026, 0.05m)],
            profitabilityPercentage: 1.10m);
}
