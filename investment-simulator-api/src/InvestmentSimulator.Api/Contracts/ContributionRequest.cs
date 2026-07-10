namespace InvestmentSimulator.Api.Contracts;

/// <summary>Additional contribution (aporte) with date and amount.</summary>
public sealed class ContributionRequest
{
    public DateOnly Date { get; init; }

    public decimal Amount { get; init; }
}
