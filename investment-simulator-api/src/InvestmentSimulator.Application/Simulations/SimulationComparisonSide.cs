using InvestmentSimulator.Domain.Enums;
using InvestmentSimulator.Domain.Results;

namespace InvestmentSimulator.Application.Simulations;

/// <summary>
/// One side of a side-by-side simulation comparison with the metrics
/// required by ERS section 24.
/// </summary>
public sealed class SimulationComparisonSide
{
    public SimulationComparisonSide(
        InvestmentType type,
        decimal netAmount,
        decimal incomeTax,
        decimal costs,
        decimal netProfit,
        decimal netReturn,
        decimal inflationAdjustedAmount)
    {
        Type = type;
        NetAmount = netAmount;
        IncomeTax = incomeTax;
        Costs = costs;
        NetProfit = netProfit;
        NetReturn = netReturn;
        InflationAdjustedAmount = inflationAdjustedAmount;
    }

    /// <summary>Investment type of this simulation (e.g. CDB or Tesouro Selic).</summary>
    public InvestmentType Type { get; }

    /// <summary>Net amount after taxes and costs in BRL.</summary>
    public decimal NetAmount { get; }

    /// <summary>Total income tax (IR) in BRL.</summary>
    public decimal IncomeTax { get; }

    /// <summary>Total costs in BRL.</summary>
    public decimal Costs { get; }

    /// <summary>Net profit in BRL.</summary>
    public decimal NetProfit { get; }

    /// <summary>
    /// Net return as a decimal fraction (e.g. 0.12 for 12%).
    /// </summary>
    public decimal NetReturn { get; }

    /// <summary>
    /// Purchasing-power-adjusted amount after inflation in BRL.
    /// </summary>
    public decimal InflationAdjustedAmount { get; }

    /// <summary>
    /// Builds a comparison side from a simulation type and its full result.
    /// </summary>
    public static SimulationComparisonSide From(InvestmentType type, SimulationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new SimulationComparisonSide(
            type,
            result.NetAmount,
            result.IncomeTax,
            result.Costs,
            result.NetProfit,
            result.NetReturn,
            result.InflationAdjustedAmount);
    }
}
