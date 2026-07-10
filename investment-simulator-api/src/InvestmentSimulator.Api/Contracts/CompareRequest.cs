namespace InvestmentSimulator.Api.Contracts;

/// <summary>Request body for <c>POST /comparar</c>.</summary>
public sealed class CompareRequest
{
    public required CompareSideRequest Left { get; init; }

    public required CompareSideRequest Right { get; init; }
}
