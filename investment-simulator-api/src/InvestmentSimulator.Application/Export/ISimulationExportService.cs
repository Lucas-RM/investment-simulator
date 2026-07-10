using InvestmentSimulator.Domain.Results;

namespace InvestmentSimulator.Application.Export;

/// <summary>
/// Exports a <see cref="SimulationResult"/> to PDF, Excel, or CSV (ERS section 25).
/// </summary>
public interface ISimulationExportService
{
    /// <summary>
    /// Generates an export document containing the summary (ERS §19)
    /// and per-contribution details (ERS §20).
    /// </summary>
    /// <param name="result">Simulation result to export.</param>
    /// <param name="format">Target file format.</param>
    /// <returns>Binary document with file name and content type.</returns>
    ExportDocument Export(SimulationResult result, ExportFormat format);
}
