using InvestmentSimulator.Domain.Results;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace InvestmentSimulator.Infrastructure.Export;

/// <summary>Builds a PDF document of simulation results (ERS section 25).</summary>
internal static class PdfSimulationExporter
{
    internal static byte[] Export(SimulationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(text => text.FontSize(10));

                page.Header().Text("Simulador de Investimentos — Resultados")
                    .SemiBold()
                    .FontSize(16);

                page.Content().PaddingVertical(16).Column(column =>
                {
                    column.Spacing(12);

                    column.Item().Text(ExportPresentation.SummarySectionTitle).SemiBold().FontSize(13);

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                        });

                        foreach (var (label, valueFactory) in ExportPresentation.SummaryRows)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                                .PaddingVertical(4).Text(label);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                                .PaddingVertical(4).AlignRight().Text(valueFactory(result));
                        }
                    });

                    column.Item().PaddingTop(8)
                        .Text(ExportPresentation.DetailsSectionTitle)
                        .SemiBold()
                        .FontSize(13);

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(0.6f);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        foreach (var header in ExportPresentation.DetailHeaders)
                        {
                            table.Cell().Background(Colors.Grey.Lighten3)
                                .Padding(4).Text(header).SemiBold();
                        }

                        foreach (var detail in result.ContributionDetails)
                        {
                            var cells = ExportPresentation.FormatDetailRow(detail);
                            foreach (var cell in cells)
                            {
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(4).Text(cell);
                            }
                        }
                    });
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Página ");
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }
}
