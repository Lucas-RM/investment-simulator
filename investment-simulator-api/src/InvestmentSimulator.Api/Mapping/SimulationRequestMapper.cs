using InvestmentSimulator.Api.Contracts;
using InvestmentSimulator.Application.Simulations;
using InvestmentSimulator.Domain.Entities;
using InvestmentSimulator.Domain.Enums;
using InvestmentSimulator.Domain.Results;

namespace InvestmentSimulator.Api.Mapping;

/// <summary>
/// Maps HTTP request contracts to domain/application models.
/// Annual rates in the API are percentages; the domain stores decimal fractions.
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
            request.StartDate,
            request.EndDate,
            MapContributions(request.Contributions),
            MapAnnualRatesFromPercent(request.CdiAnnualRates),
            MapAnnualRatesFromPercent(request.IpcaRates),
            request.CdiPercentage);

        // CDB never applies B3 custody.
        var options = new SimulationOptions();

        return (simulation, options);
    }

    public static (Simulation Simulation, SimulationOptions Options) ToTesouro(
        SimulateTesouroRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var simulation = new Simulation(
            InvestmentType.TesouroSelic,
            request.InitialAmount,
            request.StartDate,
            request.EndDate,
            MapContributions(request.Contributions),
            MapAnnualRatesFromPercent(request.SelicAnnualRates),
            MapAnnualRatesFromPercent(request.IpcaRates),
            DefaultTesouroProfitability);

        var options = new SimulationOptions
        {
            AnnualAgioRate = request.AnnualAgioRate,
            B3CustodyRates = MapOptionalAnnualRatesFromPercent(request.B3CustodyRates),
        };

        return (simulation, options);
    }

    public static (Simulation Simulation, SimulationOptions Options) ToSide(
        CompareSideRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var profitability = ResolveProfitability(request.Type, request.CdiPercentage);
        var indexRates = request.Type == InvestmentType.Cdb
            ? request.CdiAnnualRates
            : request.SelicAnnualRates;

        var simulation = new Simulation(
            request.Type,
            request.InitialAmount,
            request.StartDate,
            request.EndDate,
            MapContributions(request.Contributions),
            MapAnnualRatesFromPercent(indexRates),
            MapAnnualRatesFromPercent(request.IpcaRates),
            profitability);

        var options = new SimulationOptions
        {
            AnnualAgioRate = request.AnnualAgioRate,
            B3CustodyRates = request.Type == InvestmentType.Cdb
                ? null
                : MapOptionalAnnualRatesFromPercent(request.B3CustodyRates),
        };

        return (simulation, options);
    }

    public static SimulationHistoryEntry ToHistoryEntry(SaveHistoryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var profitability = ResolveProfitability(request.Type, request.CdiPercentage);
        var indexRates = request.Type == InvestmentType.Cdb
            ? request.CdiAnnualRates
            : request.SelicAnnualRates;

        var simulation = new Simulation(
            request.Type,
            request.InitialAmount,
            request.StartDate,
            request.EndDate,
            MapContributions(request.Contributions),
            MapAnnualRatesFromPercent(indexRates),
            MapAnnualRatesFromPercent(request.IpcaRates),
            profitability);

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
                d.GrossBalance,
                d.GrossYield,
                d.CalendarDaysInvested,
                d.BusinessDaysInvested,
                d.IncomeTax,
                d.Iof))
            .ToList();

        return new SimulationResult(
            request.StartDate,
            request.EndDate,
            request.InitialAmount,
            request.TotalAdditionalContributions,
            request.TotalInvested,
            request.GrossAmount,
            request.GrossReturnPercentage,
            request.TotalGrossYield,
            request.Costs,
            request.IncomeTax,
            request.Iof,
            request.NetAmount,
            request.NetReturnPercentage,
            request.TotalNetYield,
            request.NetAmountInflationAdjusted,
            details);
    }

    public static SimulationResultResponse ToResultResponse(SimulationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new SimulationResultResponse
        {
            StartDate = result.StartDate,
            EndDate = result.EndDate,
            InitialAmount = result.InitialAmount,
            TotalAdditionalContributions = result.TotalAdditionalContributions,
            TotalInvested = result.TotalInvested,
            GrossAmount = result.GrossAmount,
            GrossReturnPercentage = result.GrossReturnPercentage,
            TotalGrossYield = result.TotalGrossYield,
            Costs = result.Costs,
            IncomeTax = result.IncomeTax,
            Iof = result.Iof,
            NetAmount = result.NetAmount,
            NetReturnPercentage = result.NetReturnPercentage,
            TotalNetYield = result.TotalNetYield,
            NetAmountInflationAdjusted = result.NetAmountInflationAdjusted,
            ContributionDetails = result.ContributionDetails
                .Select(d => new ContributionDetailResponse
                {
                    Date = d.Date,
                    Amount = d.Amount,
                    GrossBalance = d.GrossBalance,
                    GrossYield = d.GrossYield,
                    CalendarDaysInvested = d.CalendarDaysInvested,
                    BusinessDaysInvested = d.BusinessDaysInvested,
                    IncomeTax = d.IncomeTax,
                    Iof = d.Iof,
                })
                .ToList(),
        };
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
                StartDate = simulation.InitialContributionDate,
                EndDate = simulation.EndDate,
                Contributions = simulation.Contributions
                    .Select(c => new ContributionRequest { Date = c.Date, Amount = c.Amount })
                    .ToList(),
                IndexAnnualRates = MapAnnualRatesToPercent(simulation.AnnualRates),
                IpcaRates = MapAnnualRatesToPercent(simulation.IpcaRates),
                CdiPercentage = simulation.ProfitabilityPercentage,
            },
        };
    }

    private static decimal ResolveProfitability(InvestmentType type, decimal cdiPercentage)
    {
        if (type == InvestmentType.TesouroSelic && cdiPercentage <= 0m)
        {
            return DefaultTesouroProfitability;
        }

        return cdiPercentage;
    }

    private static IReadOnlyList<Contribution> MapContributions(
        IReadOnlyList<ContributionRequest> contributions) =>
        contributions.Select(c => new Contribution(c.Date, c.Amount)).ToList();

    /// <summary>Converts API percentage rates (14.15) to domain fractions (0.1415).</summary>
    private static IReadOnlyList<AnnualRate> MapAnnualRatesFromPercent(
        IReadOnlyList<AnnualRateRequest> rates) =>
        rates.Select(r => new AnnualRate(r.Year, r.Rate / 100m)).ToList();

    private static IReadOnlyList<AnnualRate>? MapOptionalAnnualRatesFromPercent(
        IReadOnlyList<AnnualRateRequest>? rates) =>
        rates is null || rates.Count == 0
            ? null
            : MapAnnualRatesFromPercent(rates);

    /// <summary>Converts domain fractions (0.1415) back to API percentages (14.15).</summary>
    private static IReadOnlyList<AnnualRateRequest> MapAnnualRatesToPercent(
        IReadOnlyList<AnnualRate> rates) =>
        rates.Select(r => new AnnualRateRequest { Year = r.Year, Rate = r.Rate * 100m }).ToList();
}
