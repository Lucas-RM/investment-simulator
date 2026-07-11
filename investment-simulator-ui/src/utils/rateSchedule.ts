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
 * Returns a schedule switched to the given mode, regenerating per-year rows
 * from the simulation period when needed.
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

  if (mode === 'single') {
    return {
      ...schedule,
      mode,
      singleRate:
        schedule.singleRate ||
        schedule.rates.find((item) => item.rate.trim() !== '')?.rate ||
        '',
    };
  }

  const seedRates =
    schedule.rates.length > 0
      ? schedule.rates
      : schedule.singleRate.trim() !== ''
        ? generateYears(startDate, endDate).map((year) => ({
            year,
            rate: schedule.singleRate,
          }))
        : [];

  return {
    ...schedule,
    mode,
    rates: buildPerYearRates(startDate, endDate, seedRates),
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
