using InvestmentSimulator.Domain.Enums;
using InvestmentSimulator.Domain.Exceptions;

namespace InvestmentSimulator.Domain.Entities;

/// <summary>
/// Simulation input aggregate with general parameters (ERS section 3)
/// and the list of contributions (ERS section 4).
/// Monetary values use <see cref="decimal"/> (ERS section 28).
/// Validated per ERS sections 5 and 27.
/// </summary>
public sealed class Simulation
{
    public Simulation(
        InvestmentType type,
        decimal initialAmount,
        DateOnly initialContributionDate,
        DateOnly endDate,
        IReadOnlyList<Contribution> contributions,
        IReadOnlyList<AnnualRate> annualRates,
        IReadOnlyList<AnnualRate> ipcaRates,
        decimal profitabilityPercentage)
    {
        ArgumentNullException.ThrowIfNull(contributions);
        ArgumentNullException.ThrowIfNull(annualRates);
        ArgumentNullException.ThrowIfNull(ipcaRates);

        if (!Enum.IsDefined(type))
        {
            throw new DomainValidationException("Investment type is required and must be a valid value.");
        }

        if (initialAmount < 0m)
        {
            throw new DomainValidationException("Initial amount cannot be negative.");
        }

        if (initialAmount == 0m && contributions.Count == 0)
        {
            throw new DomainValidationException(
                "When initial amount is zero, at least one additional contribution is required.");
        }

        if (initialContributionDate == default)
        {
            throw new DomainValidationException("Initial contribution date is required and must be a valid date.");
        }

        if (endDate == default)
        {
            throw new DomainValidationException("End (redemption) date is required and must be a valid date.");
        }

        if (endDate < initialContributionDate)
        {
            throw new DomainValidationException("Redemption date cannot be earlier than the initial contribution date.");
        }

        if (profitabilityPercentage <= 0m)
        {
            throw new DomainValidationException("Profitability percentage must be greater than zero.");
        }

        ValidateContributions(contributions, initialContributionDate, endDate);
        ValidateAnnualRates(annualRates, nameof(annualRates));
        ValidateAnnualRates(ipcaRates, nameof(ipcaRates));

        Type = type;
        InitialAmount = initialAmount;
        InitialContributionDate = initialContributionDate;
        EndDate = endDate;
        Contributions = contributions;
        AnnualRates = annualRates;
        IpcaRates = ipcaRates;
        ProfitabilityPercentage = profitabilityPercentage;
    }

    /// <summary>Investment type (CDB or Tesouro Selic).</summary>
    public InvestmentType Type { get; }

    /// <summary>Initial investment amount in BRL (may be zero when there are additional contributions).</summary>
    public decimal InitialAmount { get; }

    /// <summary>Date of the initial contribution / simulation start.</summary>
    public DateOnly InitialContributionDate { get; }

    /// <summary>Redemption (end) date.</summary>
    public DateOnly EndDate { get; }

    /// <summary>Additional contributions during the period (ERS section 4).</summary>
    public IReadOnlyList<Contribution> Contributions { get; }

    /// <summary>
    /// Annual index rates (CDI for CDB, Selic Over for Tesouro Selic).
    /// Stored as decimal fractions (e.g. 0.15 for 15%).
    /// </summary>
    public IReadOnlyList<AnnualRate> AnnualRates { get; }

    /// <summary>
    /// Annual IPCA rates as decimal fractions (e.g. 0.05 for 5%).
    /// </summary>
    public IReadOnlyList<AnnualRate> IpcaRates { get; }

    /// <summary>
    /// Profitability percentage relative to the index
    /// (e.g. 1.10 for 110% CDI). Relevant for CDB.
    /// </summary>
    public decimal ProfitabilityPercentage { get; }

    private static void ValidateContributions(
        IReadOnlyList<Contribution> contributions,
        DateOnly initialContributionDate,
        DateOnly endDate)
    {
        DateOnly previousDate = initialContributionDate;

        for (var i = 0; i < contributions.Count; i++)
        {
            var contribution = contributions[i]
                ?? throw new DomainValidationException($"Contribution at index {i} is required.");

            if (contribution.Date < initialContributionDate)
            {
                throw new DomainValidationException(
                    $"Contribution date {contribution.Date} cannot be earlier than the initial contribution date {initialContributionDate}.");
            }

            if (contribution.Date > endDate)
            {
                throw new DomainValidationException(
                    $"Contribution date {contribution.Date} cannot be after the redemption date {endDate}.");
            }

            if (contribution.Date < previousDate)
            {
                throw new DomainValidationException(
                    "Contributions must be in chronological order.");
            }

            previousDate = contribution.Date;
        }
    }

    private static void ValidateAnnualRates(IReadOnlyList<AnnualRate> rates, string fieldName)
    {
        for (var i = 0; i < rates.Count; i++)
        {
            if (rates[i] is null)
            {
                throw new DomainValidationException($"{fieldName} entry at index {i} is required.");
            }
        }
    }
}
