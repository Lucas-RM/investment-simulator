using InvestmentSimulator.Api.Contracts;
using InvestmentSimulator.Api.Mapping;
using InvestmentSimulator.Application.History;

namespace InvestmentSimulator.Api.Endpoints;

/// <summary>Minimal API endpoints for simulation history (save / list / load).</summary>
public static class HistoryEndpoints
{
    public static IEndpointRouteBuilder MapHistoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/historico").WithTags("Histórico");

        group.MapGet(string.Empty, ListHistory).WithName("ListHistory");
        group.MapGet("/{id:guid}", GetHistory).WithName("GetHistory");
        group.MapPost(string.Empty, SaveHistory).WithName("SaveHistory");

        return app;
    }

    private static IResult ListHistory(ISimulationHistoryRepository repository)
    {
        var entries = repository.List()
            .Select(SimulationRequestMapper.ToHistoryResponse)
            .ToList();

        return Results.Ok(entries);
    }

    private static IResult GetHistory(Guid id, ISimulationHistoryRepository repository)
    {
        var entry = repository.GetById(id);
        return entry is null
            ? Results.NotFound(new { error = $"History entry '{id}' was not found." })
            : Results.Ok(SimulationRequestMapper.ToHistoryResponse(entry));
    }

    private static IResult SaveHistory(
        SaveHistoryRequest request,
        ISimulationHistoryRepository repository) =>
        DomainExceptionHandler.Execute(() =>
        {
            var entry = SimulationRequestMapper.ToHistoryEntry(request);
            var saved = repository.Save(entry);
            return Results.Created($"/historico/{saved.Id}", SimulationRequestMapper.ToHistoryResponse(saved));
        });
}
