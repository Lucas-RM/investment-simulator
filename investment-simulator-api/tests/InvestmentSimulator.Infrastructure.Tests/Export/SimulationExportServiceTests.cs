using System.Text;
using ClosedXML.Excel;
using InvestmentSimulator.Application.Export;
using InvestmentSimulator.Domain.Results;
using InvestmentSimulator.Infrastructure.Export;

namespace InvestmentSimulator.Infrastructure.Tests.Export;

public class SimulationExportServiceTests
{
    private readonly SimulationExportService _sut = new();

    [Fact]
    public void Export_Csv_ShouldIncludeSummaryAndContributionDetails()
    {
        var result = CreateSampleResult();

        var document = _sut.Export(result, ExportFormat.Csv);

        Assert.Equal(ExportFormat.Csv, document.Format);
        Assert.Equal("simulation-result.csv", document.FileName);
        Assert.Equal("text/csv; charset=utf-8", document.ContentType);
        Assert.NotEmpty(document.Content);

        var text = Encoding.UTF8.GetString(document.Content);
        Assert.Contains("Resumo da Simulação", text, StringComparison.Ordinal);
        Assert.Contains("Valor Inicial", text, StringComparison.Ordinal);
        Assert.Contains("10.000,00", text, StringComparison.Ordinal);
        Assert.Contains("Detalhamento por Aporte", text, StringComparison.Ordinal);
        Assert.Contains("01/01/2026", text, StringComparison.Ordinal);
        Assert.Contains("Valor Líquido", text, StringComparison.Ordinal);
        Assert.Contains("Valor Ajustado pela Inflação", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Export_Excel_ShouldCreateWorkbookWithSummaryAndDetailsSheets()
    {
        var result = CreateSampleResult();

        var document = _sut.Export(result, ExportFormat.Excel);

        Assert.Equal(ExportFormat.Excel, document.Format);
        Assert.Equal("simulation-result.xlsx", document.FileName);
        Assert.Equal(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            document.ContentType);
        Assert.True(document.Content.Length > 4);
        Assert.Equal((byte)'P', document.Content[0]);
        Assert.Equal((byte)'K', document.Content[1]);

        using var stream = new MemoryStream(document.Content);
        using var workbook = new XLWorkbook(stream);

        Assert.True(workbook.Worksheets.Contains("Resumo"));
        Assert.True(workbook.Worksheets.Contains("Aportes"));

        var summary = workbook.Worksheet("Resumo");
        Assert.Equal("Resumo da Simulação", summary.Cell(1, 1).GetString());
        Assert.Equal("Valor Inicial", summary.Cell(3, 1).GetString());
        Assert.Equal("10.000,00", summary.Cell(3, 2).GetString());

        var details = workbook.Worksheet("Aportes");
        Assert.Equal("Detalhamento por Aporte", details.Cell(1, 1).GetString());
        Assert.Equal("Data", details.Cell(2, 1).GetString());
        Assert.Equal("01/01/2026", details.Cell(3, 1).GetString());
        Assert.Equal("365", details.Cell(3, 3).GetString());
    }

    [Fact]
    public void Export_Pdf_ShouldGenerateNonEmptyPdfDocument()
    {
        var result = CreateSampleResult();

        var document = _sut.Export(result, ExportFormat.Pdf);

        Assert.Equal(ExportFormat.Pdf, document.Format);
        Assert.Equal("simulation-result.pdf", document.FileName);
        Assert.Equal("application/pdf", document.ContentType);
        Assert.True(document.Content.Length > 4);
        Assert.Equal((byte)'%', document.Content[0]);
        Assert.Equal((byte)'P', document.Content[1]);
        Assert.Equal((byte)'D', document.Content[2]);
        Assert.Equal((byte)'F', document.Content[3]);
    }

    [Fact]
    public void Export_ShouldThrowWhenResultIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.Export(null!, ExportFormat.Csv));
    }

    [Fact]
    public void Export_ShouldThrowWhenFormatIsUnsupported()
    {
        var result = CreateSampleResult();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => _sut.Export(result, (ExportFormat)999));
    }

    private static SimulationResult CreateSampleResult()
    {
        var contributionDetails = new List<ContributionDetail>
        {
            new(
                date: new DateOnly(2026, 1, 1),
                amount: 10_000m,
                balance: 11_000m,
                yield: 1_000m,
                daysInvested: 365,
                incomeTax: 150m,
                iof: 0m),
            new(
                date: new DateOnly(2026, 2, 1),
                amount: 900m,
                balance: 980m,
                yield: 80m,
                daysInvested: 334,
                incomeTax: 16m,
                iof: 0m),
        };

        return new SimulationResult(
            initialAmount: 10_000m,
            contributionsAmount: 900m,
            totalInvested: 10_900m,
            grossAmount: 11_980m,
            grossReturn: 0.0990825688m,
            costs: 12.50m,
            incomeTax: 166m,
            iof: 0m,
            netAmount: 11_801.50m,
            netReturn: 0.0827064220m,
            netProfit: 901.50m,
            inflationAdjustedAmount: 11_239.52m,
            contributionDetails: contributionDetails);
    }
}
