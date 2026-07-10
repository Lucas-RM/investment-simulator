using InvestmentSimulator.Domain.Exceptions;
using InvestmentSimulator.Domain.Rates;

namespace InvestmentSimulator.Domain.Calculation;

/// <summary>
/// Holds annual rate schedules and exposes the rates in force for the current year.
/// Automatically switches CDI/Selic (index), IPCA and B3 rates when the year changes
/// (ERS section 11).
/// </summary>
public sealed class SimulationRateContext
{
    private readonly RateSchedule _indexRates;
    private readonly RateSchedule _ipcaRates;
    private readonly RateSchedule? _b3Rates;

    public SimulationRateContext(
        RateSchedule indexRates,
        RateSchedule ipcaRates,
        RateSchedule? b3Rates = null)
    {
        ArgumentNullException.ThrowIfNull(indexRates);
        ArgumentNullException.ThrowIfNull(ipcaRates);

        _indexRates = indexRates;
        _ipcaRates = ipcaRates;
        _b3Rates = b3Rates;
    }

    /// <summary>Calendar year whose rates are currently loaded. Null before the first <see cref="AdvanceToYear"/>.</summary>
    public int? CurrentYear { get; private set; }

    /// <summary>Index annual rate (CDI or Selic Over) for <see cref="CurrentYear"/>.</summary>
    public decimal CurrentIndexAnnualRate { get; private set; }

    /// <summary>Index daily rate for <see cref="CurrentYear"/>.</summary>
    public decimal CurrentIndexDailyRate { get; private set; }

    /// <summary>IPCA annual rate for <see cref="CurrentYear"/>.</summary>
    public decimal CurrentIpcaAnnualRate { get; private set; }

    /// <summary>IPCA daily rate for <see cref="CurrentYear"/>.</summary>
    public decimal CurrentIpcaDailyRate { get; private set; }

    /// <summary>B3 custody annual rate for <see cref="CurrentYear"/> (0 when no B3 schedule was provided).</summary>
    public decimal CurrentB3AnnualRate { get; private set; }

    /// <summary>B3 custody daily rate for <see cref="CurrentYear"/> (0 when no B3 schedule was provided).</summary>
    public decimal CurrentB3DailyRate { get; private set; }

    /// <summary>
    /// Loads rates for <paramref name="year"/> when it differs from <see cref="CurrentYear"/>.
    /// </summary>
    /// <returns><c>true</c> when rates were switched (including the first load).</returns>
    public bool AdvanceToYear(int year)
    {
        if (year < 1)
        {
            throw new DomainValidationException("Year must be a valid positive calendar year.");
        }

        if (CurrentYear == year)
        {
            return false;
        }

        CurrentYear = year;
        CurrentIndexAnnualRate = _indexRates.GetAnnualRate(year);
        CurrentIndexDailyRate = _indexRates.GetDailyRate(year);
        CurrentIpcaAnnualRate = _ipcaRates.GetAnnualRate(year);
        CurrentIpcaDailyRate = _ipcaRates.GetDailyRate(year);

        if (_b3Rates is null)
        {
            CurrentB3AnnualRate = 0m;
            CurrentB3DailyRate = 0m;
        }
        else
        {
            CurrentB3AnnualRate = _b3Rates.GetAnnualRate(year);
            CurrentB3DailyRate = _b3Rates.GetDailyRate(year);
        }

        return true;
    }
}
