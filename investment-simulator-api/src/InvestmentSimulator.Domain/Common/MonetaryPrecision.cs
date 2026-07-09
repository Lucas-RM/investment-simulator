namespace InvestmentSimulator.Domain.Common;

/// <summary>
/// Precision rules for monetary and percentage calculations (ERS section 28).
/// All monetary values must use <see cref="decimal"/> — never float or double.
/// </summary>
public static class MonetaryPrecision
{
    /// <summary>Minimum decimal places for intermediate calculation storage.</summary>
    public const int IntermediateDecimalPlaces = 8;

    /// <summary>Decimal places for currency presentation (BRL).</summary>
    public const int CurrencyDecimalPlaces = 2;

    /// <summary>Decimal places for percentage presentation.</summary>
    public const int PercentageDecimalPlaces = 4;
}
