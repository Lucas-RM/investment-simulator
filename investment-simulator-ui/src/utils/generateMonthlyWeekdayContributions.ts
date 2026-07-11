import type { ContributionInput } from '@/types/contribution';
import { isValidIsoDate } from '@/utils/isoDate';

/** Business weekdays supported by the recurring contributions generator. */
export type RecurringWeekday =
  'monday' | 'tuesday' | 'wednesday' | 'thursday' | 'friday';

export const RECURRING_WEEKDAY_LABELS: Record<RecurringWeekday, string> = {
  monday: 'Primeira segunda-feira do mês',
  tuesday: 'Primeira terça-feira do mês',
  wednesday: 'Primeira quarta-feira do mês',
  thursday: 'Primeira quinta-feira do mês',
  friday: 'Primeira sexta-feira do mês',
};

const WEEKDAY_TO_JS: Record<RecurringWeekday, number> = {
  monday: 1,
  tuesday: 2,
  wednesday: 3,
  thursday: 4,
  friday: 5,
};

function toIsoDate(year: number, month: number, day: number): string {
  const mm = String(month).padStart(2, '0');
  const dd = String(day).padStart(2, '0');
  return `${year}-${mm}-${dd}`;
}

/**
 * Returns the calendar day (1–31) of the first occurrence of
 * `weekday` in the given month (UTC).
 */
export function firstWeekdayDayOfMonth(
  year: number,
  month: number,
  weekday: RecurringWeekday,
): number {
  const target = WEEKDAY_TO_JS[weekday];
  // Day-of-week for the 1st of the month (UTC).
  const firstDow = new Date(Date.UTC(year, month - 1, 1)).getUTCDay();
  const delta = (target - firstDow + 7) % 7;
  return 1 + delta;
}

/**
 * Generates one contribution on the first chosen business weekday of each
 * month between startDate and endDate (inclusive), all with the same amount.
 * Dates outside the period are skipped.
 */
export function generateMonthlyWeekdayContributions(options: {
  startDate: string;
  endDate: string;
  weekday: RecurringWeekday;
  amount: string;
}): ContributionInput[] {
  const { startDate, endDate, weekday, amount } = options;

  if (!isValidIsoDate(startDate) || !isValidIsoDate(endDate)) {
    return [];
  }

  if (endDate < startDate) {
    return [];
  }

  const [startYear, startMonth] = startDate.split('-').map(Number);
  const [endYear, endMonth] = endDate.split('-').map(Number);

  const contributions: ContributionInput[] = [];
  let year = startYear;
  let month = startMonth;

  while (year < endYear || (year === endYear && month <= endMonth)) {
    const day = firstWeekdayDayOfMonth(year, month, weekday);
    const date = toIsoDate(year, month, day);

    if (date >= startDate && date <= endDate) {
      contributions.push({ date, amount });
    }

    month += 1;
    if (month > 12) {
      month = 1;
      year += 1;
    }
  }

  return contributions;
}
