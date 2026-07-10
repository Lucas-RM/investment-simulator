namespace InvestmentSimulator.Domain.Results;

/// <summary>
/// Final simulation summary with aggregated totals and per-contribution details
/// (ERS sections 19 and 20). Monetary values use <see cref="decimal"/> (ERS section 28).
/// </summary>
public sealed class SimulationResult
{
    public SimulationResult(
        decimal initialAmount,
        decimal totalAdditionalContributions,
        decimal totalInvested,
        decimal grossAmount,
        decimal grossReturnPercentage,
        decimal costs,
        decimal incomeTax,
        decimal iof,
        decimal netAmount,
        decimal netReturnPercentage,
        decimal totalNetYield,
        decimal netAmountInflationAdjusted,
        IReadOnlyList<ContributionDetail> contributionDetails)
    {
        InitialAmount = initialAmount;
        TotalAdditionalContributions = totalAdditionalContributions;
        TotalInvested = totalInvested;
        GrossAmount = grossAmount;
        GrossReturnPercentage = grossReturnPercentage;
        Costs = costs;
        IncomeTax = incomeTax;
        Iof = iof;
        NetAmount = netAmount;
        NetReturnPercentage = netReturnPercentage;
        TotalNetYield = totalNetYield;
        NetAmountInflationAdjusted = netAmountInflationAdjusted;
        ContributionDetails = contributionDetails;
    }

    /// <summary>Initial investment amount in BRL.</summary>
    public decimal InitialAmount { get; }

    /// <summary>Sum of additional contributions (aportes) in BRL.</summary>
    public decimal TotalAdditionalContributions { get; }

    /// <summary>Total invested amount (initial + contributions) in BRL.</summary>
    public decimal TotalInvested { get; }

    /// <summary>Gross amount before taxes and costs in BRL.</summary>
    public decimal GrossAmount { get; }

    /// <summary>
    /// Gross return as a decimal fraction (e.g. 0.15 for 15%).
    /// </summary>
    public decimal GrossReturnPercentage { get; }

    /// <summary>Total costs (e.g. B3 custody) in BRL.</summary>
    public decimal Costs { get; }

    /// <summary>Total income tax (IR) in BRL.</summary>
    public decimal IncomeTax { get; }

    /// <summary>Total IOF in BRL.</summary>
    public decimal Iof { get; }

    /// <summary>Net amount after taxes and costs in BRL.</summary>
    public decimal NetAmount { get; }

    /// <summary>
    /// Net return as a decimal fraction (e.g. 0.12 for 12%).
    /// </summary>
    public decimal NetReturnPercentage { get; }

    /// <summary>Total net yield (lucro líquido) in BRL.</summary>
    public decimal TotalNetYield { get; }

    /// <summary>
    /// Purchasing-power-adjusted net amount after inflation (ERS section 18) in BRL.
    /// </summary>
    public decimal NetAmountInflationAdjusted { get; }

    /// <summary>Per-contribution breakdown (ERS section 20).</summary>
    public IReadOnlyList<ContributionDetail> ContributionDetails { get; }
}
