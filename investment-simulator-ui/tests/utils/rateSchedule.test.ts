import {
  buildPerYearRates,
  createEmptyRateSchedule,
  switchRateScheduleMode,
  syncRateScheduleYears,
} from '@/utils/rateSchedule';

describe('rateSchedule helpers', () => {
  it('creates an empty single-rate schedule', () => {
    expect(createEmptyRateSchedule()).toEqual({
      mode: 'single',
      singleRate: '',
      rates: [],
    });
  });

  it('builds per-year rows for the period and preserves existing rates', () => {
    expect(
      buildPerYearRates('2026-08-10', '2028-01-01', [
        { year: 2026, rate: '15' },
        { year: 2027, rate: '13' },
      ]),
    ).toEqual([
      { year: 2026, rate: '15' },
      { year: 2027, rate: '13' },
      { year: 2028, rate: '' },
    ]);
  });

  it('switches from single to per-year seeding every year with the single rate', () => {
    const next = switchRateScheduleMode(
      { mode: 'single', singleRate: '14.15', rates: [] },
      'perYear',
      '2026-01-01',
      '2027-12-31',
    );

    expect(next.mode).toBe('perYear');
    expect(next.rates).toEqual([
      { year: 2026, rate: '14.15' },
      { year: 2027, rate: '14.15' },
    ]);
  });

  it('syncs per-year rows when the period changes', () => {
    const synced = syncRateScheduleYears(
      {
        mode: 'perYear',
        singleRate: '',
        rates: [
          { year: 2026, rate: '15' },
          { year: 2027, rate: '13' },
          { year: 2028, rate: '11' },
        ],
      },
      '2027-01-01',
      '2028-06-01',
    );

    expect(synced.rates).toEqual([
      { year: 2027, rate: '13' },
      { year: 2028, rate: '11' },
    ]);
  });
});
