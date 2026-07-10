namespace InvestmentSimulator.Api.Contracts;

/// <summary>Corpo da requisição de <c>POST /comparar</c>.</summary>
public sealed class CompareRequest
{
    /// <summary>Simulação do lado esquerdo da comparação.</summary>
    public required CompareSideRequest Left { get; init; }

    /// <summary>Simulação do lado direito da comparação.</summary>
    public required CompareSideRequest Right { get; init; }
}
