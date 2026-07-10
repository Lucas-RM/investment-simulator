using InvestmentSimulator.Api.Contracts;
using InvestmentSimulator.Api.Mapping;
using InvestmentSimulator.Application.Export;

namespace InvestmentSimulator.Api.Endpoints;

/// <summary>Minimal API endpoint for exporting simulation results.</summary>
public static class ExportEndpoints
{
    public static IEndpointRouteBuilder MapExportEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/exportar", Export)
            .WithName("ExportSimulation")
            .WithTags("Exportação");

        return app;
    }

    private static IResult Export(
        ExportRequest request,
        ISimulationExportService exportService) =>
        DomainExceptionHandler.Execute(() =>
        {
            var result = SimulationRequestMapper.ToResult(request.Result);
            var document = exportService.Export(result, request.Format);
            return Results.File(document.Content, document.ContentType, document.FileName);
        });
}
