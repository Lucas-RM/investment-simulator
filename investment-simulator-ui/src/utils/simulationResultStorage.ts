import { InvestmentType } from '@/types/investment';
import type { SimulationResultResponse } from '@/types/simulationApi';

const STORAGE_PREFIX = 'investment-simulator:result:';

export function resultStorageKey(investmentType: InvestmentType): string {
  return `${STORAGE_PREFIX}${investmentType}`;
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}

function isFiniteNumber(value: unknown): value is number {
  return typeof value === 'number' && Number.isFinite(value);
}

function isSimulationResult(value: unknown): value is SimulationResultResponse {
  if (!isRecord(value)) {
    return false;
  }

  const requiredNumbers = [
    'initialAmount',
    'totalAdditionalContributions',
    'totalInvested',
    'grossAmount',
    'grossReturnPercentage',
    'costs',
    'incomeTax',
    'iof',
    'netAmount',
    'netReturnPercentage',
    'totalNetYield',
    'netAmountInflationAdjusted',
  ] as const;

  for (const key of requiredNumbers) {
    if (!isFiniteNumber(value[key])) {
      return false;
    }
  }

  return Array.isArray(value.contributionDetails);
}

/** Persists the latest simulation result for the given investment type. */
export function saveSimulationResult(
  investmentType: InvestmentType,
  result: SimulationResultResponse,
): void {
  try {
    sessionStorage.setItem(
      resultStorageKey(investmentType),
      JSON.stringify(result),
    );
  } catch {
    // Ignore quota / private-mode failures.
  }
}

/** Loads the latest simulation result, or null when missing/invalid. */
export function loadSimulationResult(
  investmentType: InvestmentType,
): SimulationResultResponse | null {
  try {
    const raw = sessionStorage.getItem(resultStorageKey(investmentType));
    if (!raw) {
      return null;
    }

    const parsed: unknown = JSON.parse(raw);
    return isSimulationResult(parsed) ? parsed : null;
  } catch {
    return null;
  }
}

/** Clears the stored simulation result for the investment type. */
export function clearSimulationResult(investmentType: InvestmentType): void {
  try {
    sessionStorage.removeItem(resultStorageKey(investmentType));
  } catch {
    // Ignore storage failures.
  }
}
