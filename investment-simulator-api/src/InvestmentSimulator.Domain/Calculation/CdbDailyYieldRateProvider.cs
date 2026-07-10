using InvestmentSimulator.Domain.Exceptions;

namespace InvestmentSimulator.Domain.Calculation;

/// <summary>
/// CDB yield strategy for the daily calculation engine (ERS section 12).
/// Applies <c>daily CDI × contracted profitability</c> as the effective daily rate.
/// </summary>
public sealed class CdbDailyYieldRateProvider : IDailyYieldRateProvider
{
    private readonly decimal _profitabilityPercentage;

    /// <param name="profitabilityPercentage">
    /// Contracted profitability relative to CDI (e.g. 1.10 for 110% CDI). Must be greater than zero.
    /// </param>
    public CdbDailyYieldRateProvider(decimal profitabilityPercentage)
    {
        if (profitabilityPercentage <= 0m)
        {
            throw new DomainValidationException("Profitability percentage must be greater than zero.");
        }

        _profitabilityPercentage = profitabilityPercentage;
    }

    /// <summary>Contracted profitability relative to CDI (e.g. 1.10 for 110% CDI).</summary>
    public decimal ProfitabilityPercentage => _profitabilityPercentage;

    /// <inheritdoc />
    public decimal GetDailyYieldRate(SimulationRateContext rateContext)
    {
        ArgumentNullException.ThrowIfNull(rateContext);

        return CdbCalculator.CalculateDailyYieldRate(
            rateContext.CurrentIndexDailyRate,
            _profitabilityPercentage);
    }
}
