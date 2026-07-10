namespace InvestmentSimulator.Domain.Calculation;

/// <summary>
/// Supplies the effective daily yield rate used by the base calculation engine.
/// CDB uses <see cref="CdbDailyYieldRateProvider"/> (ERS section 12);
/// Tesouro Selic is plugged in by a later commit (ERS section 13).
/// </summary>
public interface IDailyYieldRateProvider
{
    /// <summary>
    /// Returns the effective daily yield rate (decimal fraction) given the rates
    /// currently loaded in <paramref name="rateContext"/>.
    /// </summary>
    decimal GetDailyYieldRate(SimulationRateContext rateContext);
}
