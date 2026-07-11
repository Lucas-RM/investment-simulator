import type { AnnualRateInput, RateScheduleInput } from '@/types/rates';
import { generateYears } from '@/utils/generateYears';

/** Creates an empty single-rate schedule. */
export function createEmptyRateSchedule(): RateScheduleInput {
  return {
    mode: 'single',
    singleRate: '',
    rates: [],
  };
}

/** Builds per-year rows for the given period, preserving rates for years that remain. */
export function buildPerYearRates(
  startDate: string,
  endDate: string,
  previous: AnnualRateInput[] = [],
): AnnualRateInput[] {
  const previousByYear = new Map(
    previous.map((item) => [item.year, item.rate]),
  );

  return generateYears(startDate, endDate).map((year) => ({
    year,
    rate: previousByYear.get(year) ?? '',
  }));
}

/**
 * Returns a schedule switched to the given mode.
 * The inactive mode's data is always cleared:
 * - single → per-year: copies the single rate into every year, then clears singleRate
 * - per-year → single: uses the first filled year as singleRate, then clears rates
 */
export function switchRateScheduleMode(
  schedule: RateScheduleInput,
  mode: RateScheduleInput['mode'],
  startDate: string,
  endDate: string,
): RateScheduleInput {
  if (mode === schedule.mode) {
    return schedule;
  }

  const years = generateYears(startDate, endDate);

  if (mode === 'single') {
    const singleRate =
      schedule.rates.find((item) => item.rate.trim() !== '')?.rate ?? '';

    return {
      ...schedule,
      mode,
      singleRate,
      rates: [],
    };
  }

  const singleRate = schedule.singleRate.trim();

  return {
    ...schedule,
    mode,
    singleRate: '',
    rates: years.map((year) => ({
      year,
      rate: singleRate,
    })),
  };
}

/** Ensures per-year rows match the current period when already in per-year mode. */
export function syncRateScheduleYears(
  schedule: RateScheduleInput,
  startDate: string,
  endDate: string,
): RateScheduleInput {
  if (schedule.mode !== 'perYear') {
    return schedule;
  }

  return {
    ...schedule,
    rates: buildPerYearRates(startDate, endDate, schedule.rates),
  };
}
