using InvestmentSimulator.Api.Contracts;
using InvestmentSimulator.Api.Mapping;
using InvestmentSimulator.Application.Simulations;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentSimulator.Api.Controllers;

/// <summary>Controllers HTTP para simulação de CDB e Tesouro Selic.</summary>
[ApiController]
[Route("simular")]
[Tags("Simulação")]
public sealed class SimulationController : ControllerBase
{
    private readonly SimulationService _simulationService;

    public SimulationController(SimulationService simulationService)
    {
        _simulationService = simulationService;
    }

    /// <summary>Simula um CDB pós-fixado (CDI × percentual contratado).</summary>
    [HttpPost("cdb")]
    [ProducesResponseType(typeof(SimulationResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult SimulateCdb([FromBody] SimulateCdbRequest request) =>
        DomainExceptionHandler.Execute(() =>
        {
            var (simulation, options) = SimulationRequestMapper.ToCdb(request);
            var result = _simulationService.Run(simulation, options);
            return Ok(SimulationRequestMapper.ToResultResponse(result));
        });

    /// <summary>Simula Tesouro Selic (Selic + ágio/deságio).</summary>
    [HttpPost("tesouro")]
    [ProducesResponseType(typeof(SimulationResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult SimulateTesouro([FromBody] SimulateTesouroRequest request) =>
        DomainExceptionHandler.Execute(() =>
        {
            var (simulation, options) = SimulationRequestMapper.ToTesouro(request);
            var result = _simulationService.Run(simulation, options);
            return Ok(SimulationRequestMapper.ToResultResponse(result));
        });
}
