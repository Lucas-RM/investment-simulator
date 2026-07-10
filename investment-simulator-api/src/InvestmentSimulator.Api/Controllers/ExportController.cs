using InvestmentSimulator.Api.Contracts;
using InvestmentSimulator.Api.Mapping;
using InvestmentSimulator.Application.Export;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentSimulator.Api.Controllers;

/// <summary>HTTP controller for exporting simulation results.</summary>
[ApiController]
[Route("exportar")]
[Tags("Exportação")]
public sealed class ExportController : ControllerBase
{
    private readonly ISimulationExportService _exportService;

    public ExportController(ISimulationExportService exportService)
    {
        _exportService = exportService;
    }

    /// <summary>Exporta um resultado de simulação em CSV, Excel ou PDF.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Export([FromBody] ExportRequest request) =>
        DomainExceptionHandler.Execute(() =>
        {
            var result = SimulationRequestMapper.ToResult(request.Result);
            var document = _exportService.Export(result, request.Format);
            return File(document.Content, document.ContentType, document.FileName);
        });
}
