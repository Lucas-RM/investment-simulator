namespace InvestmentSimulator.Domain.Calculation;

/// <summary>
/// Supplies the effective daily yield rate used by the base calculation engine.
/// Concrete strategies (CDB, Tesouro Selic) are plugged in by later commits (ERS sections 12–13).
/// </summary>
public interface IDailyYieldRateProvider
{
    /// <summary>
    /// Returns the effective daily yield rate (decimal fraction) given the rates
    /// currently loaded in <paramref name="rateContext"/>.
    /// </summary>
    decimal GetDailyYieldRate(SimulationRateContext rateContext);
}
