using InvestmentSimulator.Domain.Exceptions;
using InvestmentSimulator.Domain.Rates;

namespace InvestmentSimulator.Domain.Calculation;

/// <summary>
/// Tesouro Selic yield strategy for the daily calculation engine (ERS section 13).
/// Applies <c>(1 + daily Selic) × (1 + daily ágio) − 1</c> as the effective daily rate.
/// </summary>
public sealed class TesouroSelicDailyYieldRateProvider : IDailyYieldRateProvider
{
    private readonly decimal _annualAgioRate;
    private readonly decimal _dailyAgioRate;

    /// <param name="annualAgioRate">
    /// Annual premium (positive) or discount (negative) rate as a decimal fraction
    /// (e.g. 0.001 for +0.1% ágio). Must be greater than −1.
    /// </param>
    public TesouroSelicDailyYieldRateProvider(decimal annualAgioRate)
    {
        if (annualAgioRate <= -1m)
        {
            throw new DomainValidationException(
                "Annual ágio rate must be greater than -1 so that (1 + rate) is positive.");
        }

        _annualAgioRate = annualAgioRate;
        _dailyAgioRate = RateConverter.AnnualToDaily(annualAgioRate);
    }

    /// <summary>Annual premium/discount rate as a decimal fraction.</summary>
    public decimal AnnualAgioRate => _annualAgioRate;

    /// <summary>Daily equivalent of <see cref="AnnualAgioRate"/> (ERS section 8).</summary>
    public decimal DailyAgioRate => _dailyAgioRate;

    /// <inheritdoc />
    public decimal GetDailyYieldRate(SimulationRateContext rateContext)
    {
        ArgumentNullException.ThrowIfNull(rateContext);

        return TesouroSelicCalculator.CalculateDailyYieldRate(
            rateContext.CurrentIndexDailyRate,
            _dailyAgioRate);
    }
}
