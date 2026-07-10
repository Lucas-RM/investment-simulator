namespace InvestmentSimulator.Domain.Rates;

/// <summary>
/// How annual rates are supplied for a simulation period (ERS section 6).
/// </summary>
public enum RateEntryMode
{
    /// <summary>One rate applied to every year in the period.</summary>
    SingleRate = 0,

    /// <summary>A distinct rate for each calendar year in the period.</summary>
    PerYear = 1,
}
