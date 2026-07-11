import { isValidIsoDate } from '@/utils/isoDate';

/**
 * Returns every calendar year from startDate through endDate, inclusive
 * (ERS section 7 — used when the user chooses year-by-year rate entry).
 *
 * @example
 * generateYears('2026-08-10', '2031-04-15')
 * // → [2026, 2027, 2028, 2029, 2030, 2031]
 */
export function generateYears(startDate: string, endDate: string): number[] {
  if (!isValidIsoDate(startDate) || !isValidIsoDate(endDate)) {
    return [];
  }

  if (endDate < startDate) {
    return [];
  }

  const startYear = Number(startDate.slice(0, 4));
  const endYear = Number(endDate.slice(0, 4));
  const years: number[] = [];

  for (let year = startYear; year <= endYear; year += 1) {
    years.push(year);
  }

  return years;
}
