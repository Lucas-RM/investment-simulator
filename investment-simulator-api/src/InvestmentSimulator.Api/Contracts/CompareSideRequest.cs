using InvestmentSimulator.Domain.Enums;

namespace InvestmentSimulator.Api.Contracts;

/// <summary>One side of a comparison request (<c>POST /comparar</c>).</summary>
public sealed class CompareSideRequest
{
    public InvestmentType Type { get; init; }

    public decimal InitialAmount { get; init; }

    public DateOnly InitialContributionDate { get; init; }

    public DateOnly EndDate { get; init; }

    public IReadOnlyList<ContributionRequest> Contributions { get; init; } = [];

    public IReadOnlyList<AnnualRateRequest> AnnualRates { get; init; } = [];

    public IReadOnlyList<AnnualRateRequest> IpcaRates { get; init; } = [];

    /// <summary>
    /// Required for CDB (e.g. 1.10 = 110% CDI). For Tesouro Selic defaults to 1 when omitted/zero.
    /// </summary>
    public decimal ProfitabilityPercentage { get; init; }

    /// <summary>Annual ágio/deságio for Tesouro Selic. Ignored for CDB.</summary>
    public decimal AnnualAgioRate { get; init; }

    public decimal Costs { get; init; }

    public IReadOnlyList<AnnualRateRequest>? B3Rates { get; init; }
}
