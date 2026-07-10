namespace InvestmentSimulator.Domain.Calculation;

/// <summary>
/// Minimal yield provider that applies the current index daily rate as-is
/// (CDI or Selic Over without CDB profitability or Tesouro ágio adjustments).
/// Prefer <see cref="CdbDailyYieldRateProvider"/> for CDB (ERS section 12)
/// and <see cref="TesouroSelicDailyYieldRateProvider"/> for Tesouro Selic (ERS section 13).
/// </summary>
public sealed class IndexDailyYieldRateProvider : IDailyYieldRateProvider
{
    public decimal GetDailyYieldRate(SimulationRateContext rateContext)
    {
        ArgumentNullException.ThrowIfNull(rateContext);
        return rateContext.CurrentIndexDailyRate;
    }
}
