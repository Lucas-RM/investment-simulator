using InvestmentSimulator.Api.Contracts;
using InvestmentSimulator.Api.Mapping;
using InvestmentSimulator.Application.Simulations;

namespace InvestmentSimulator.Api.Endpoints;

/// <summary>Minimal API endpoint for side-by-side simulation comparison.</summary>
public static class ComparisonEndpoints
{
    public static IEndpointRouteBuilder MapComparisonEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/comparar", Compare)
            .WithName("CompareSimulations")
            .WithTags("Comparação");

        return app;
    }

    private static IResult Compare(
        CompareRequest request,
        SimulationComparisonService comparisonService) =>
        DomainExceptionHandler.Execute(() =>
        {
            var (left, leftOptions) = SimulationRequestMapper.ToSide(request.Left);
            var (right, rightOptions) = SimulationRequestMapper.ToSide(request.Right);
            var result = comparisonService.Compare(left, right, leftOptions, rightOptions);
            return Results.Ok(result);
        });
}
