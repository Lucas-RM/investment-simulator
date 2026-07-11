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

  it('switches from single to per-year seeding every year and clears singleRate', () => {
    const next = switchRateScheduleMode(
      { mode: 'single', singleRate: '14.15', rates: [] },
      'perYear',
      '2026-01-01',
      '2027-12-31',
    );

    expect(next.mode).toBe('perYear');
    expect(next.singleRate).toBe('');
    expect(next.rates).toEqual([
      { year: 2026, rate: '14.15' },
      { year: 2027, rate: '14.15' },
    ]);
  });

  it('overwrites previous per-year rates with the current single rate and clears it', () => {
    const next = switchRateScheduleMode(
      {
        mode: 'single',
        singleRate: '12',
        rates: [
          { year: 2026, rate: '15' },
          { year: 2027, rate: '13' },
        ],
      },
      'perYear',
      '2026-01-01',
      '2027-12-31',
    );

    expect(next.singleRate).toBe('');
    expect(next.rates).toEqual([
      { year: 2026, rate: '12' },
      { year: 2027, rate: '12' },
    ]);
  });

  it('creates empty per-year rows when single rate is empty and clears leftover rates source', () => {
    const next = switchRateScheduleMode(
      {
        mode: 'single',
        singleRate: '',
        rates: [
          { year: 2026, rate: '15' },
          { year: 2027, rate: '13' },
        ],
      },
      'perYear',
      '2026-01-01',
      '2027-12-31',
    );

    expect(next.singleRate).toBe('');
    expect(next.rates).toEqual([
      { year: 2026, rate: '' },
      { year: 2027, rate: '' },
    ]);
  });

  it('switches back to single using the first filled year and clears per-year rates', () => {
    const next = switchRateScheduleMode(
      {
        mode: 'perYear',
        singleRate: '',
        rates: [
          { year: 2026, rate: '15' },
          { year: 2027, rate: '12' },
        ],
      },
      'single',
      '2026-01-01',
      '2027-12-31',
    );

    expect(next.mode).toBe('single');
    expect(next.singleRate).toBe('15');
    expect(next.rates).toEqual([]);
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
