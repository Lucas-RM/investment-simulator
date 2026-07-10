using InvestmentSimulator.Domain.Common;

namespace InvestmentSimulator.Application.Simulations;

/// <summary>
/// Side-by-side comparison of two simulations (ERS section 24),
/// including absolute differences (right − left) for each metric.
/// </summary>
public sealed class SimulationComparisonResult
{
    public SimulationComparisonResult(
        SimulationComparisonSide left,
        SimulationComparisonSide right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        Left = left;
        Right = right;
        NetAmountDifference = Diff(right.NetAmount, left.NetAmount);
        IncomeTaxDifference = Diff(right.IncomeTax, left.IncomeTax);
        CostsDifference = Diff(right.Costs, left.Costs);
        TotalNetYieldDifference = Diff(right.TotalNetYield, left.TotalNetYield);
        NetReturnPercentageDifference = Diff(right.NetReturnPercentage, left.NetReturnPercentage);
        NetAmountInflationAdjustedDifference = Diff(
            right.NetAmountInflationAdjusted,
            left.NetAmountInflationAdjusted);
    }

    /// <summary>First simulation in the comparison (e.g. CDB).</summary>
    public SimulationComparisonSide Left { get; }

    /// <summary>Second simulation in the comparison (e.g. Tesouro Selic).</summary>
    public SimulationComparisonSide Right { get; }

    /// <summary>Right net amount minus left net amount in BRL.</summary>
    public decimal NetAmountDifference { get; }

    /// <summary>Right income tax minus left income tax in BRL.</summary>
    public decimal IncomeTaxDifference { get; }

    /// <summary>Right costs minus left costs in BRL.</summary>
    public decimal CostsDifference { get; }

    /// <summary>Right total net yield minus left total net yield in BRL.</summary>
    public decimal TotalNetYieldDifference { get; }

    /// <summary>Right net return percentage minus left net return percentage (decimal fraction).</summary>
    public decimal NetReturnPercentageDifference { get; }

    /// <summary>
    /// Right inflation-adjusted net amount minus left inflation-adjusted net amount in BRL.
    /// </summary>
    public decimal NetAmountInflationAdjustedDifference { get; }

    private static decimal Diff(decimal right, decimal left) =>
        Math.Round(
            right - left,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);
}
