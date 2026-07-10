using InvestmentSimulator.Api.Contracts;
using InvestmentSimulator.Application.Simulations;
using InvestmentSimulator.Domain.Entities;
using InvestmentSimulator.Domain.Enums;
using InvestmentSimulator.Domain.Results;

namespace InvestmentSimulator.Api.Mapping;

/// <summary>
/// Maps HTTP request contracts to domain/application models.
/// </summary>
public static class SimulationRequestMapper
{
    /// <summary>Default profitability for Tesouro Selic (domain requires a positive value).</summary>
    private const decimal DefaultTesouroProfitability = 1m;

    public static (Simulation Simulation, SimulationOptions Options) ToCdb(
        SimulateCdbRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var simulation = new Simulation(
            InvestmentType.Cdb,
            request.InitialAmount,
            request.InitialContributionDate,
            request.EndDate,
            MapContributions(request.Contributions),
            MapAnnualRates(request.AnnualRates),
            MapAnnualRates(request.IpcaRates),
            request.ProfitabilityPercentage,
            request.Costs);

        var options = new SimulationOptions
        {
            B3Rates = MapOptionalAnnualRates(request.B3Rates),
        };

        return (simulation, options);
    }

    public static (Simulation Simulation, SimulationOptions Options) ToTesouro(
        SimulateTesouroRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var simulation = new Simulation(
            InvestmentType.TesouroSelic,
            request.InitialAmount,
            request.InitialContributionDate,
            request.EndDate,
            MapContributions(request.Contributions),
            MapAnnualRates(request.AnnualRates),
            MapAnnualRates(request.IpcaRates),
            DefaultTesouroProfitability,
            request.Costs);

        var options = new SimulationOptions
        {
            AnnualAgioRate = request.AnnualAgioRate,
            B3Rates = MapOptionalAnnualRates(request.B3Rates),
        };

        return (simulation, options);
    }

    public static (Simulation Simulation, SimulationOptions Options) ToSide(
        CompareSideRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var profitability = ResolveProfitability(request.Type, request.ProfitabilityPercentage);

        var simulation = new Simulation(
            request.Type,
            request.InitialAmount,
            request.InitialContributionDate,
            request.EndDate,
            MapContributions(request.Contributions),
            MapAnnualRates(request.AnnualRates),
            MapAnnualRates(request.IpcaRates),
            profitability,
            request.Costs);

        var options = new SimulationOptions
        {
            AnnualAgioRate = request.AnnualAgioRate,
            B3Rates = MapOptionalAnnualRates(request.B3Rates),
        };

        return (simulation, options);
    }

    public static SimulationHistoryEntry ToHistoryEntry(SaveHistoryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var profitability = ResolveProfitability(request.Type, request.ProfitabilityPercentage);

        var simulation = new Simulation(
            request.Type,
            request.InitialAmount,
            request.InitialContributionDate,
            request.EndDate,
            MapContributions(request.Contributions),
            MapAnnualRates(request.AnnualRates),
            MapAnnualRates(request.IpcaRates),
            profitability,
            request.Costs);

        return new SimulationHistoryEntry(
            request.Name,
            request.Date,
            request.Observations,
            simulation,
            request.Id);
    }

    public static SimulationResult ToResult(SimulationResultRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var details = request.ContributionDetails
            .Select(d => new ContributionDetail(
                d.Date,
                d.Amount,
                d.Balance,
                d.Yield,
                d.DaysInvested,
                d.IncomeTax,
                d.Iof))
            .ToList();

        return new SimulationResult(
            request.InitialAmount,
            request.ContributionsAmount,
            request.TotalInvested,
            request.GrossAmount,
            request.GrossReturn,
            request.Costs,
            request.IncomeTax,
            request.Iof,
            request.NetAmount,
            request.NetReturn,
            request.NetProfit,
            request.InflationAdjustedAmount,
            details);
    }

    public static HistoryEntryResponse ToHistoryResponse(SimulationHistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var simulation = entry.Simulation;

        return new HistoryEntryResponse
        {
            Id = entry.Id,
            Name = entry.Name,
            Date = entry.Date,
            Type = entry.Type,
            Observations = entry.Observations,
            Simulation = new SimulationSnapshotResponse
            {
                Type = simulation.Type,
                InitialAmount = simulation.InitialAmount,
                InitialContributionDate = simulation.InitialContributionDate,
                EndDate = simulation.EndDate,
                Contributions = simulation.Contributions
                    .Select(c => new ContributionRequest { Date = c.Date, Amount = c.Amount })
                    .ToList(),
                AnnualRates = simulation.AnnualRates
                    .Select(r => new AnnualRateRequest { Year = r.Year, Rate = r.Rate })
                    .ToList(),
                IpcaRates = simulation.IpcaRates
                    .Select(r => new AnnualRateRequest { Year = r.Year, Rate = r.Rate })
                    .ToList(),
                ProfitabilityPercentage = simulation.ProfitabilityPercentage,
                Costs = simulation.Costs,
            },
        };
    }

    private static decimal ResolveProfitability(InvestmentType type, decimal profitabilityPercentage)
    {
        if (type == InvestmentType.TesouroSelic && profitabilityPercentage <= 0m)
        {
            return DefaultTesouroProfitability;
        }

        return profitabilityPercentage;
    }

    private static IReadOnlyList<Contribution> MapContributions(
        IReadOnlyList<ContributionRequest> contributions) =>
        contributions.Select(c => new Contribution(c.Date, c.Amount)).ToList();

    private static IReadOnlyList<AnnualRate> MapAnnualRates(
        IReadOnlyList<AnnualRateRequest> rates) =>
        rates.Select(r => new AnnualRate(r.Year, r.Rate)).ToList();

    private static IReadOnlyList<AnnualRate>? MapOptionalAnnualRates(
        IReadOnlyList<AnnualRateRequest>? rates) =>
        rates is null || rates.Count == 0
            ? null
            : MapAnnualRates(rates);
}
