using InvestmentSimulator.Application.Export;

namespace InvestmentSimulator.Api.Contracts;

/// <summary>Corpo da requisição de <c>POST /exportar</c>.</summary>
public sealed class ExportRequest
{
    /// <summary>Formato de exportação: <c>Csv</c>, <c>Excel</c> ou <c>Pdf</c>.</summary>
    public ExportFormat Format { get; init; }

    /// <summary>Resultado da simulação a exportar (mesmo shape do status 200 de <c>/simular/*</c>).</summary>
    public required SimulationResultRequest Result { get; init; }
}
