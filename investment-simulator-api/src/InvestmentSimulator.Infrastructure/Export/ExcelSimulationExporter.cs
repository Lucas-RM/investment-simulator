using ClosedXML.Excel;
using InvestmentSimulator.Domain.Results;

namespace InvestmentSimulator.Infrastructure.Export;

/// <summary>Builds an .xlsx workbook of simulation results (ERS section 25).</summary>
internal static class ExcelSimulationExporter
{
    internal static byte[] Export(SimulationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        using var workbook = new XLWorkbook();

        var summarySheet = workbook.Worksheets.Add("Resumo");
        WriteSummary(summarySheet, result);

        var detailsSheet = workbook.Worksheets.Add("Aportes");
        WriteDetails(detailsSheet, result);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void WriteSummary(IXLWorksheet sheet, SimulationResult result)
    {
        sheet.Cell(1, 1).Value = ExportPresentation.SummarySectionTitle;
        sheet.Cell(1, 1).Style.Font.Bold = true;

        sheet.Cell(2, 1).Value = "Campo";
        sheet.Cell(2, 2).Value = "Valor";
        sheet.Range(2, 1, 2, 2).Style.Font.Bold = true;

        var row = 3;
        foreach (var (label, valueFactory) in ExportPresentation.SummaryRows)
        {
            sheet.Cell(row, 1).Value = label;
            sheet.Cell(row, 2).Value = valueFactory(result);
            row++;
        }

        sheet.Columns().AdjustToContents();
    }

    private static void WriteDetails(IXLWorksheet sheet, SimulationResult result)
    {
        sheet.Cell(1, 1).Value = ExportPresentation.DetailsSectionTitle;
        sheet.Cell(1, 1).Style.Font.Bold = true;

        for (var column = 0; column < ExportPresentation.DetailHeaders.Length; column++)
        {
            var cell = sheet.Cell(2, column + 1);
            cell.Value = ExportPresentation.DetailHeaders[column];
            cell.Style.Font.Bold = true;
        }

        var row = 3;
        foreach (var detail in result.ContributionDetails)
        {
            var cells = ExportPresentation.FormatDetailRow(detail);
            for (var column = 0; column < cells.Length; column++)
            {
                sheet.Cell(row, column + 1).Value = cells[column];
            }

            row++;
        }

        sheet.Columns().AdjustToContents();
    }
}
