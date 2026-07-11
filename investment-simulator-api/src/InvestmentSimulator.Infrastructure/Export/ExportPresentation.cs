using System.Globalization;
using InvestmentSimulator.Domain.Common;
using InvestmentSimulator.Domain.Results;

namespace InvestmentSimulator.Infrastructure.Export;

/// <summary>
/// Shared presentation helpers for export formats (ERS §25 / §28).
/// Rounds only at presentation: 2 places for currency, 4 for percentages.
/// </summary>
internal static class ExportPresentation
{
    internal static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("pt-BR");

    internal const string SummarySectionTitle = "Resumo da Simulação";
    internal const string DetailsSectionTitle = "Detalhamento por Aporte";

    internal static readonly (string Label, Func<SimulationResult, string> Value)[] SummaryRows =
    [
        ("Data Inicial", r => FormatDate(r.StartDate)),
        ("Data de Resgate", r => FormatDate(r.EndDate)),
        ("Valor Inicial", r => FormatCurrency(r.InitialAmount)),
        ("Aportes", r => FormatCurrency(r.TotalAdditionalContributions)),
        ("Total Investido", r => FormatCurrency(r.TotalInvested)),
        ("Valor Bruto", r => FormatCurrency(r.GrossAmount)),
        ("Rentabilidade Bruta", r => FormatPercentage(r.GrossReturnPercentage)),
        ("Lucro Bruto", r => FormatCurrency(r.TotalGrossYield)),
        ("Custos", r => FormatCurrency(r.Costs)),
        ("IR", r => FormatCurrency(r.IncomeTax)),
        ("IOF", r => FormatCurrency(r.Iof)),
        ("Valor Líquido", r => FormatCurrency(r.NetAmount)),
        ("Rentabilidade Líquida", r => FormatPercentage(r.NetReturnPercentage)),
        ("Lucro Líquido", r => FormatCurrency(r.TotalNetYield)),
        ("Valor Ajustado pela Inflação", r => FormatCurrency(r.NetAmountInflationAdjusted)),
    ];

    internal static readonly string[] DetailHeaders =
    [
        "Data",
        "Valor",
        "Dias",
        "IR",
        "IOF",
        "Saldo",
    ];

    internal static string FormatCurrency(decimal value) =>
        RoundCurrency(value).ToString("N2", Culture);

    internal static string FormatPercentage(decimal fraction) =>
        RoundPercentage(fraction * 100m).ToString("N4", Culture) + "%";

    internal static string FormatDate(DateOnly date) =>
        date.ToString("dd/MM/yyyy", Culture);

    internal static string FormatInteger(int value) =>
        value.ToString(Culture);

    internal static decimal RoundCurrency(decimal value) =>
        Math.Round(value, MonetaryPrecision.CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);

    internal static decimal RoundPercentage(decimal percentagePoints) =>
        Math.Round(
            percentagePoints,
            MonetaryPrecision.PercentageDecimalPlaces,
            MidpointRounding.AwayFromZero);

    internal static string[] FormatDetailRow(ContributionDetail detail) =>
    [
        FormatDate(detail.Date),
        FormatCurrency(detail.Amount),
        FormatInteger(detail.CalendarDaysInvested),
        FormatCurrency(detail.IncomeTax),
        FormatCurrency(detail.Iof),
        FormatCurrency(detail.GrossBalance),
    ];
}
