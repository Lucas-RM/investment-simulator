namespace InvestmentSimulator.Domain.Enums;

/// <summary>
/// How to treat contributions dated on weekends or national holidays (ERS section 29).
/// </summary>
public enum NonBusinessDayContributionRule
{
    /// <summary>
    /// Postpone the contribution to the next business day.
    /// </summary>
    PostponeToNextBusinessDay = 1,

    /// <summary>
    /// Keep the original date even if it is not a business day.
    /// </summary>
    KeepOriginalDate = 2,
}
