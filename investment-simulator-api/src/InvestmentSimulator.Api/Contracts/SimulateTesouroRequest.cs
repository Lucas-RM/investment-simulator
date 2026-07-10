namespace InvestmentSimulator.Api.Contracts;

/// <summary>Request body for <c>POST /simular/tesouro</c>.</summary>
public sealed class SimulateTesouroRequest
{
    public decimal InitialAmount { get; init; }

    public DateOnly InitialContributionDate { get; init; }

    public DateOnly EndDate { get; init; }

    public IReadOnlyList<ContributionRequest> Contributions { get; init; } = [];

    /// <summary>Annual Selic Over rates as decimal fractions.</summary>
    public IReadOnlyList<AnnualRateRequest> AnnualRates { get; init; } = [];

    /// <summary>Annual IPCA rates as decimal fractions.</summary>
    public IReadOnlyList<AnnualRateRequest> IpcaRates { get; init; } = [];

    /// <summary>
    /// Annual ágio/deságio as a decimal fraction (e.g. 0.001 = +0.1%; negative = deságio).
    /// </summary>
    public decimal AnnualAgioRate { get; init; }

    public decimal Costs { get; init; }

    /// <summary>Optional annual B3 custody rates. Omitted or empty skips B3.</summary>
    public IReadOnlyList<AnnualRateRequest>? B3Rates { get; init; }
}
