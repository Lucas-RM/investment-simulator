using InvestmentSimulator.Domain.Enums;

namespace InvestmentSimulator.Domain.Entities;

/// <summary>
/// Simulation input aggregate with general parameters (ERS section 3)
/// and the list of contributions (ERS section 4).
/// Monetary values use <see cref="decimal"/> (ERS section 28).
/// </summary>
public sealed class Simulation
{
    public Simulation(
        InvestmentType type,
        decimal initialAmount,
        DateOnly initialContributionDate,
        DateOnly endDate,
        IReadOnlyList<Contribution> contributions,
        IReadOnlyList<AnnualRate> annualRates,
        IReadOnlyList<AnnualRate> ipcaRates,
        decimal profitabilityPercentage,
        decimal costs)
    {
        Type = type;
        InitialAmount = initialAmount;
        InitialContributionDate = initialContributionDate;
        EndDate = endDate;
        Contributions = contributions;
        AnnualRates = annualRates;
        IpcaRates = ipcaRates;
        ProfitabilityPercentage = profitabilityPercentage;
        Costs = costs;
    }

    /// <summary>Investment type (CDB or Tesouro Selic).</summary>
    public InvestmentType Type { get; }

    /// <summary>Initial investment amount in BRL.</summary>
    public decimal InitialAmount { get; }

    /// <summary>Date of the initial contribution.</summary>
    public DateOnly InitialContributionDate { get; }

    /// <summary>Redemption (end) date.</summary>
    public DateOnly EndDate { get; }

    /// <summary>Additional contributions during the period (ERS section 4).</summary>
    public IReadOnlyList<Contribution> Contributions { get; }

    /// <summary>
    /// Annual index rates (CDI for CDB, Selic Over for Tesouro Selic).
    /// Stored as decimal fractions (e.g. 0.15 for 15%).
    /// </summary>
    public IReadOnlyList<AnnualRate> AnnualRates { get; }

    /// <summary>
    /// Annual IPCA rates as decimal fractions (e.g. 0.05 for 5%).
    /// </summary>
    public IReadOnlyList<AnnualRate> IpcaRates { get; }

    /// <summary>
    /// Profitability percentage relative to the index
    /// (e.g. 1.10 for 110% CDI). Relevant for CDB.
    /// </summary>
    public decimal ProfitabilityPercentage { get; }

    /// <summary>Costs associated with the simulation (e.g. custody fees).</summary>
    public decimal Costs { get; }
}
