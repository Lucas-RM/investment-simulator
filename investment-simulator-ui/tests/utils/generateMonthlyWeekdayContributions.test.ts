import {
  firstWeekdayDayOfMonth,
  generateMonthlyWeekdayContributions,
} from '@/utils/generateMonthlyWeekdayContributions';

describe('generateMonthlyWeekdayContributions', () => {
  it('finds the first business weekday of a month', () => {
    // January 2026: Thu 1, Fri 2, Sat 3, Sun 4, Mon 5, Tue 6, Wed 7
    expect(firstWeekdayDayOfMonth(2026, 1, 'monday')).toBe(5)
    expect(firstWeekdayDayOfMonth(2026, 1, 'tuesday')).toBe(6)
    expect(firstWeekdayDayOfMonth(2026, 1, 'wednesday')).toBe(7)
    expect(firstWeekdayDayOfMonth(2026, 1, 'thursday')).toBe(1)
    expect(firstWeekdayDayOfMonth(2026, 1, 'friday')).toBe(2)
  })
  it('generates first Mondays within the simulation period', () => {
    const contributions = generateMonthlyWeekdayContributions({
      startDate: '2026-01-01',
      endDate: '2026-04-30',
      weekday: 'monday',
      amount: '500',
    });

    expect(contributions).toEqual([
      { date: '2026-01-05', amount: '500' },
      { date: '2026-02-02', amount: '500' },
      { date: '2026-03-02', amount: '500' },
      { date: '2026-04-06', amount: '500' },
    ]);
  });

  it('generates first Thursdays within the simulation period', () => {
    const contributions = generateMonthlyWeekdayContributions({
      startDate: '2026-01-01',
      endDate: '2026-03-31',
      weekday: 'thursday',
      amount: '1000.50',
    });

    expect(contributions).toEqual([
      { date: '2026-01-01', amount: '1000.50' },
      { date: '2026-02-05', amount: '1000.50' },
      { date: '2026-03-05', amount: '1000.50' },
    ]);
  });

  it('generates first Fridays within the simulation period', () => {
    const contributions = generateMonthlyWeekdayContributions({
      startDate: '2026-01-01',
      endDate: '2026-03-31',
      weekday: 'friday',
      amount: '250',
    })

    expect(contributions).toEqual([
      { date: '2026-01-02', amount: '250' },
      { date: '2026-02-06', amount: '250' },
      { date: '2026-03-06', amount: '250' },
    ])
  })

  it('skips dates before startDate or after endDate', () => {
    // First Monday of Jan 2026 is day 5; period starts on day 10.
    const contributions = generateMonthlyWeekdayContributions({
      startDate: '2026-01-10',
      endDate: '2026-02-01',
      weekday: 'monday',
      amount: '200',
    });

    // Feb 2 is after endDate 2026-02-01, so only empty if Jan skipped and Feb out
    expect(contributions).toEqual([]);
  });

  it('includes the boundary month when the weekday falls on endDate', () => {
    const contributions = generateMonthlyWeekdayContributions({
      startDate: '2026-02-02',
      endDate: '2026-02-02',
      weekday: 'monday',
      amount: '100',
    });

    expect(contributions).toEqual([{ date: '2026-02-02', amount: '100' }]);
  });

  it('returns an empty list for invalid dates', () => {
    expect(
      generateMonthlyWeekdayContributions({
        startDate: '',
        endDate: '2026-12-31',
        weekday: 'monday',
        amount: '100',
      }),
    ).toEqual([]);
  });
});
