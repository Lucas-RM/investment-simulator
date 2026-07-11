import type { ContributionInput } from '@/types/contribution';
import type { RateScheduleInput } from '@/types/rates';
import type {
  AnnualRateRequest,
  ContributionRequest,
  SimulateCdbRequest,
  SimulateTesouroRequest,
} from '@/types/simulationApi';
import type {
  CdbSimulationDraft,
  TesouroSimulationDraft,
} from '@/types/simulationDraft';
import { generateYears } from '@/utils/generateYears';
import { parseDecimalString } from '@/utils/parseDecimal';

function requireDecimal(value: string, fieldLabel: string): number {
  const parsed = parseDecimalString(value);
  if (parsed === null) {
    throw new Error(`Invalid decimal for ${fieldLabel}.`);
  }
  return parsed;
}

function mapContributions(
  contributions: ContributionInput[],
): ContributionRequest[] {
  return contributions.map((item) => ({
    date: item.date,
    amount: requireDecimal(item.amount, 'contribution amount'),
  }));
}

/**
 * Expands a UI rate schedule into the API annual-rate list (percent values).
 * Single mode → one entry (API expands it for the period).
 * Per-year mode → one entry per year.
 */
export function mapRateScheduleToAnnualRates(
  schedule: RateScheduleInput,
  startDate: string,
  endDate: string,
): AnnualRateRequest[] {
  if (schedule.mode === 'single') {
    const rate = requireDecimal(schedule.singleRate, 'annual rate');
    const years = generateYears(startDate, endDate);
    const year = years[0] ?? Number(startDate.slice(0, 4));
    return [{ year, rate }];
  }

  return schedule.rates.map((entry) => ({
    year: entry.year,
    rate: requireDecimal(entry.rate, `rate for ${entry.year}`),
  }));
}

/**
 * Builds POST /simular/cdb body from a validated CDB draft.
 */
export function mapCdbDraftToRequest(
  draft: CdbSimulationDraft,
): SimulateCdbRequest {
  if (!draft.rates) {
    throw new Error('CDB rates are required before calling the API.');
  }

  const { startDate, endDate, initialAmount } = draft.generalInputs;

  const profitabilityPercent = requireDecimal(
    draft.rates.profitabilityPercentage,
    'profitability',
  );

  return {
    initialAmount: requireDecimal(initialAmount, 'initial amount'),
    startDate,
    endDate,
    contributions: mapContributions(draft.contributions),
    cdiAnnualRates: mapRateScheduleToAnnualRates(
      draft.rates.cdi,
      startDate,
      endDate,
    ),
    ipcaRates: mapRateScheduleToAnnualRates(
      draft.rates.ipca,
      startDate,
      endDate,
    ),
    cdiPercentage: profitabilityPercent / 100,
  };
}

/**
 * Builds POST /simular/tesouro body from a validated Tesouro draft.
 * Ágio UI is a percentage string; API expects a decimal fraction.
 */
export function mapTesouroDraftToRequest(
  draft: TesouroSimulationDraft,
): SimulateTesouroRequest {
  if (!draft.rates) {
    throw new Error('Tesouro rates are required before calling the API.');
  }

  const { startDate, endDate, initialAmount } = draft.generalInputs;
  const agioPercent = requireDecimal(
    draft.rates.annualAgioRate,
    'ágio/deságio',
  );

  const b3Rates = mapRateScheduleToAnnualRates(
    draft.rates.b3Custody,
    startDate,
    endDate,
  );
  const b3AllZero = b3Rates.every((entry) => entry.rate === 0);

  return {
    initialAmount: requireDecimal(initialAmount, 'initial amount'),
    startDate,
    endDate,
    contributions: mapContributions(draft.contributions),
    selicAnnualRates: mapRateScheduleToAnnualRates(
      draft.rates.selic,
      startDate,
      endDate,
    ),
    ipcaRates: mapRateScheduleToAnnualRates(
      draft.rates.ipca,
      startDate,
      endDate,
    ),
    annualAgioRate: agioPercent / 100,
    b3CustodyRates: b3AllZero ? null : b3Rates,
  };
}
