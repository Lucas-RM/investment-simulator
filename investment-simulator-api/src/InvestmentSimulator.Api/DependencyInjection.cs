using InvestmentSimulator.Application.Export;
using InvestmentSimulator.Application.History;
using InvestmentSimulator.Application.Simulations;
using InvestmentSimulator.Domain.Calendar;
using InvestmentSimulator.Infrastructure.Export;
using InvestmentSimulator.Infrastructure.Persistence;

namespace InvestmentSimulator.Api;

/// <summary>
/// Registers Application and Infrastructure services used by the Minimal API.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds simulation, comparison, export, and history services to the container.
    /// </summary>
    public static IServiceCollection AddInvestmentSimulator(this IServiceCollection services)
    {
        services.AddSingleton<FinancialCalendar>();
        services.AddSingleton<SimulationService>();
        services.AddSingleton(sp =>
            new SimulationComparisonService(sp.GetRequiredService<SimulationService>()));
        services.AddSingleton<ISimulationExportService, SimulationExportService>();
        services.AddSingleton<ISimulationHistoryRepository, InMemorySimulationHistoryRepository>();

        return services;
    }
}
