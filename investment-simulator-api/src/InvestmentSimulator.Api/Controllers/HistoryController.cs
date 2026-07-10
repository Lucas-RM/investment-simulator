using InvestmentSimulator.Api.Contracts;
using InvestmentSimulator.Api.Mapping;
using InvestmentSimulator.Application.History;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentSimulator.Api.Controllers;

/// <summary>HTTP controllers for simulation history (save / list / load).</summary>
[ApiController]
[Route("historico")]
[Tags("Histórico")]
public sealed class HistoryController : ControllerBase
{
    private readonly ISimulationHistoryRepository _repository;

    public HistoryController(ISimulationHistoryRepository repository)
    {
        _repository = repository;
    }

    /// <summary>Lista entradas salvas do histórico.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<HistoryEntryResponse>), StatusCodes.Status200OK)]
    public IActionResult ListHistory()
    {
        var entries = _repository.List()
            .Select(SimulationRequestMapper.ToHistoryResponse)
            .ToList();

        return Ok(entries);
    }

    /// <summary>Carrega uma entrada do histórico pelo identificador.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(HistoryEntryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetHistory(Guid id)
    {
        var entry = _repository.GetById(id);
        return entry is null
            ? NotFound(new { error = $"History entry '{id}' was not found." })
            : Ok(SimulationRequestMapper.ToHistoryResponse(entry));
    }

    /// <summary>Salva (ou sobrescreve) uma simulação no histórico.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(HistoryEntryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult SaveHistory([FromBody] SaveHistoryRequest request) =>
        DomainExceptionHandler.Execute(() =>
        {
            var entry = SimulationRequestMapper.ToHistoryEntry(request);
            var saved = _repository.Save(entry);
            return Created($"/historico/{saved.Id}", SimulationRequestMapper.ToHistoryResponse(saved));
        });
}
