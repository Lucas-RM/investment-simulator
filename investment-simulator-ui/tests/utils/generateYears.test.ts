import { generateYears } from '@/utils/generateYears';

describe('generateYears', () => {
  it('returns inclusive calendar years between two dates', () => {
    expect(generateYears('2026-08-10', '2031-04-15')).toEqual([
      2026, 2027, 2028, 2029, 2030, 2031,
    ]);
  });

  it('returns a single year when start and end are in the same year', () => {
    expect(generateYears('2026-01-01', '2026-12-31')).toEqual([2026]);
  });

  it('returns an empty list for invalid or inverted dates', () => {
    expect(generateYears('', '2026-01-01')).toEqual([]);
    expect(generateYears('2026-02-30', '2027-01-01')).toEqual([]);
    expect(generateYears('2027-01-01', '2026-01-01')).toEqual([]);
  });
});
