using System.Text;
using InvestmentSimulator.Domain.Results;

namespace InvestmentSimulator.Infrastructure.Export;

/// <summary>Builds a UTF-8 CSV export of simulation results (ERS section 25).</summary>
internal static class CsvSimulationExporter
{
    private const char Separator = ';';

    internal static byte[] Export(SimulationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var builder = new StringBuilder();

        builder.AppendLine(Escape(ExportPresentation.SummarySectionTitle));
        builder.Append(Escape("Campo")).Append(Separator).AppendLine(Escape("Valor"));

        foreach (var (label, valueFactory) in ExportPresentation.SummaryRows)
        {
            builder
                .Append(Escape(label))
                .Append(Separator)
                .AppendLine(Escape(valueFactory(result)));
        }

        builder.AppendLine();
        builder.AppendLine(Escape(ExportPresentation.DetailsSectionTitle));
        builder.AppendLine(string.Join(Separator, ExportPresentation.DetailHeaders.Select(Escape)));

        foreach (var detail in result.ContributionDetails)
        {
            var cells = ExportPresentation.FormatDetailRow(detail);
            builder.AppendLine(string.Join(Separator, cells.Select(Escape)));
        }

        // BOM helps Excel detect UTF-8 correctly on Windows.
        var utf8WithBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        return utf8WithBom.GetBytes(builder.ToString());
    }

    private static string Escape(string value)
    {
        if (value.Contains('"') || value.Contains(Separator) || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return value;
    }
}
