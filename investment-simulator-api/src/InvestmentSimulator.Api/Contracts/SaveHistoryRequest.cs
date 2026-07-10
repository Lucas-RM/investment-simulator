using InvestmentSimulator.Domain.Enums;

namespace InvestmentSimulator.Api.Contracts;

/// <summary>Request body for <c>POST /historico</c>.</summary>
public sealed class SaveHistoryRequest
{
    public string Name { get; init; } = string.Empty;

    public DateOnly Date { get; init; }

    public string Observations { get; init; } = string.Empty;

    /// <summary>Optional id to overwrite an existing history entry.</summary>
    public Guid? Id { get; init; }

    public InvestmentType Type { get; init; }

    public decimal InitialAmount { get; init; }

    public DateOnly InitialContributionDate { get; init; }

    public DateOnly EndDate { get; init; }

    public IReadOnlyList<ContributionRequest> Contributions { get; init; } = [];

    public IReadOnlyList<AnnualRateRequest> AnnualRates { get; init; } = [];

    public IReadOnlyList<AnnualRateRequest> IpcaRates { get; init; } = [];

    /// <summary>
    /// Required for CDB. For Tesouro Selic defaults to 1 when omitted/zero.
    /// </summary>
    public decimal ProfitabilityPercentage { get; init; }

    public decimal Costs { get; init; }
}
