using InvestmentSimulator.Api.Contracts;
using InvestmentSimulator.Api.Mapping;
using InvestmentSimulator.Application.Simulations;

namespace InvestmentSimulator.Api.Endpoints;

/// <summary>Minimal API endpoints for CDB and Tesouro Selic simulation.</summary>
public static class SimulationEndpoints
{
    public static IEndpointRouteBuilder MapSimulationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/simular/cdb", SimulateCdb)
            .WithName("SimulateCdb")
            .WithTags("Simulação");

        app.MapPost("/simular/tesouro", SimulateTesouro)
            .WithName("SimulateTesouro")
            .WithTags("Simulação");

        return app;
    }

    private static IResult SimulateCdb(
        SimulateCdbRequest request,
        SimulationService simulationService) =>
        DomainExceptionHandler.Execute(() =>
        {
            var (simulation, options) = SimulationRequestMapper.ToCdb(request);
            var result = simulationService.Run(simulation, options);
            return Results.Ok(result);
        });

    private static IResult SimulateTesouro(
        SimulateTesouroRequest request,
        SimulationService simulationService) =>
        DomainExceptionHandler.Execute(() =>
        {
            var (simulation, options) = SimulationRequestMapper.ToTesouro(request);
            var result = simulationService.Run(simulation, options);
            return Results.Ok(result);
        });
}
