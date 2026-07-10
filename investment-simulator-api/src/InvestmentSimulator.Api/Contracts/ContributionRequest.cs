namespace InvestmentSimulator.Api.Contracts;

/// <summary>Aporte adicional com data e valor.</summary>
public sealed class ContributionRequest
{
    /// <summary>Data do aporte.</summary>
    public DateOnly Date { get; init; }

    /// <summary>Valor do aporte em R$ (deve ser maior que zero).</summary>
    public decimal Amount { get; init; }
}
