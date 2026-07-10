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
        NetProfitDifference = Diff(right.NetProfit, left.NetProfit);
        NetReturnDifference = Diff(right.NetReturn, left.NetReturn);
        InflationAdjustedAmountDifference = Diff(
            right.InflationAdjustedAmount,
            left.InflationAdjustedAmount);
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

    /// <summary>Right net profit minus left net profit in BRL.</summary>
    public decimal NetProfitDifference { get; }

    /// <summary>Right net return minus left net return (decimal fraction).</summary>
    public decimal NetReturnDifference { get; }

    /// <summary>
    /// Right inflation-adjusted amount minus left inflation-adjusted amount in BRL.
    /// </summary>
    public decimal InflationAdjustedAmountDifference { get; }

    private static decimal Diff(decimal right, decimal left) =>
        Math.Round(
            right - left,
            MonetaryPrecision.IntermediateDecimalPlaces,
            MidpointRounding.AwayFromZero);
}
