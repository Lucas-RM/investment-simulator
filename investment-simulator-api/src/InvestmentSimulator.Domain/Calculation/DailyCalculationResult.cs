namespace InvestmentSimulator.Domain.Calculation;

/// <summary>
/// Outcome of a <see cref="DailyCalculationEngine"/> run: updated contribution
/// positions and how many times annual rates were switched (ERS sections 9–11).
/// </summary>
public sealed class DailyCalculationResult
{
    public DailyCalculationResult(
        IReadOnlyList<ContributionPosition> positions,
        int rateSwitchCount)
    {
        ArgumentNullException.ThrowIfNull(positions);

        if (rateSwitchCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rateSwitchCount), "Rate switch count cannot be negative.");
        }

        Positions = positions;
        RateSwitchCount = rateSwitchCount;
    }

    /// <summary>Contribution positions after the daily loop.</summary>
    public IReadOnlyList<ContributionPosition> Positions { get; }

    /// <summary>
    /// Number of times rates were loaded/switched (first year load + each year change).
    /// </summary>
    public int RateSwitchCount { get; }

    /// <summary>Sum of current balances across all positions.</summary>
    public decimal TotalBalance => Positions.Sum(p => p.Balance);

    /// <summary>Sum of accumulated yields across all positions.</summary>
    public decimal TotalYield => Positions.Sum(p => p.Yield);
}
