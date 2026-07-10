namespace InvestmentSimulator.Api.Contracts;

/// <summary>Request body for <c>POST /simular/cdb</c>.</summary>
public sealed class SimulateCdbRequest
{
    public decimal InitialAmount { get; init; }

    public DateOnly InitialContributionDate { get; init; }

    public DateOnly EndDate { get; init; }

    public IReadOnlyList<ContributionRequest> Contributions { get; init; } = [];

    /// <summary>Annual CDI rates as decimal fractions.</summary>
    public IReadOnlyList<AnnualRateRequest> AnnualRates { get; init; } = [];

    /// <summary>Annual IPCA rates as decimal fractions.</summary>
    public IReadOnlyList<AnnualRateRequest> IpcaRates { get; init; } = [];

    /// <summary>Profitability vs CDI (e.g. 1.10 = 110% CDI).</summary>
    public decimal ProfitabilityPercentage { get; init; }

    public decimal Costs { get; init; }

    /// <summary>Optional annual B3 custody rates. Omitted or empty skips B3.</summary>
    public IReadOnlyList<AnnualRateRequest>? B3Rates { get; init; }
}
