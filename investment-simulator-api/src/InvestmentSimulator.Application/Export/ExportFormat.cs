namespace InvestmentSimulator.Application.Export;

/// <summary>Supported simulation result export formats (ERS section 25).</summary>
public enum ExportFormat
{
    Csv = 1,
    Excel = 2,
    Pdf = 3,
}
