using InvestmentSimulator.Application.Export;

namespace InvestmentSimulator.Api.Contracts;

/// <summary>Request body for <c>POST /exportar</c>.</summary>
public sealed class ExportRequest
{
    public ExportFormat Format { get; init; }

    public required SimulationResultRequest Result { get; init; }
}
