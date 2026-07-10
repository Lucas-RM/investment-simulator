using InvestmentSimulator.Application.Export;
using InvestmentSimulator.Domain.Results;

namespace InvestmentSimulator.Infrastructure.Export;

/// <summary>
/// Generates PDF, Excel, and CSV exports of simulation results (ERS section 25).
/// </summary>
public sealed class SimulationExportService : ISimulationExportService
{
    private const string DefaultBaseFileName = "simulation-result";

    /// <inheritdoc />
    public ExportDocument Export(SimulationResult result, ExportFormat format)
    {
        ArgumentNullException.ThrowIfNull(result);

        return format switch
        {
            ExportFormat.Csv => new ExportDocument(
                ExportFormat.Csv,
                $"{DefaultBaseFileName}.csv",
                "text/csv; charset=utf-8",
                CsvSimulationExporter.Export(result)),

            ExportFormat.Excel => new ExportDocument(
                ExportFormat.Excel,
                $"{DefaultBaseFileName}.xlsx",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ExcelSimulationExporter.Export(result)),

            ExportFormat.Pdf => new ExportDocument(
                ExportFormat.Pdf,
                $"{DefaultBaseFileName}.pdf",
                "application/pdf",
                PdfSimulationExporter.Export(result)),

            _ => throw new ArgumentOutOfRangeException(
                nameof(format),
                format,
                $"Unsupported export format: {format}."),
        };
    }
}
