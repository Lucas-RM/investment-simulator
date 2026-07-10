using InvestmentSimulator.Api.Contracts;
using InvestmentSimulator.Api.Mapping;
using InvestmentSimulator.Application.Simulations;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentSimulator.Api.Controllers;

/// <summary>HTTP controller for side-by-side simulation comparison.</summary>
[ApiController]
[Route("comparar")]
[Tags("Comparação")]
public sealed class ComparisonController : ControllerBase
{
    private readonly SimulationComparisonService _comparisonService;

    public ComparisonController(SimulationComparisonService comparisonService)
    {
        _comparisonService = comparisonService;
    }

    /// <summary>Compara duas simulações lado a lado.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(SimulationComparisonResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Compare([FromBody] CompareRequest request) =>
        DomainExceptionHandler.Execute(() =>
        {
            var (left, leftOptions) = SimulationRequestMapper.ToSide(request.Left);
            var (right, rightOptions) = SimulationRequestMapper.ToSide(request.Right);
            var result = _comparisonService.Compare(left, right, leftOptions, rightOptions);
            return Ok(result);
        });
}
