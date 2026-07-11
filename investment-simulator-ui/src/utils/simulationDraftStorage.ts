import { InvestmentType } from '@/types/investment';
import {
  createEmptyDraft,
  type CdbSimulationDraft,
  type SimulationDraft,
  type TesouroSimulationDraft,
} from '@/types/simulationDraft';

const STORAGE_PREFIX = 'investment-simulator:draft:';

export function draftStorageKey(investmentType: InvestmentType): string {
  return `${STORAGE_PREFIX}${investmentType}`;
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}

function isContributionList(
  value: unknown,
): value is SimulationDraft['contributions'] {
  if (!Array.isArray(value)) {
    return false;
  }

  return value.every(
    (item) =>
      isRecord(item) &&
      typeof item.date === 'string' &&
      typeof item.amount === 'string',
  );
}

function parseGeneralInputs(
  value: unknown,
  fallback: SimulationDraft['generalInputs'],
): SimulationDraft['generalInputs'] {
  if (!isRecord(value)) {
    return fallback;
  }

  return {
    initialAmount:
      typeof value.initialAmount === 'string'
        ? value.initialAmount
        : fallback.initialAmount,
    startDate:
      typeof value.startDate === 'string'
        ? value.startDate
        : fallback.startDate,
    endDate:
      typeof value.endDate === 'string' ? value.endDate : fallback.endDate,
  };
}

function isValidDraft(
  value: unknown,
  investmentType: InvestmentType,
): value is SimulationDraft {
  if (!isRecord(value) || value.version !== 1) {
    return false;
  }

  if (value.investmentType !== investmentType) {
    return false;
  }

  if (!isRecord(value.generalInputs)) {
    return false;
  }

  if (!isContributionList(value.contributions)) {
    return false;
  }

  if (typeof value.contributionsConfirmed !== 'boolean') {
    return false;
  }

  return true;
}

/** Reads a simulation draft from localStorage, or returns an empty draft. */
export function loadSimulationDraft(
  investmentType: InvestmentType,
): SimulationDraft {
  const empty = createEmptyDraft(investmentType);

  try {
    const raw = localStorage.getItem(draftStorageKey(investmentType));
    if (!raw) {
      return empty;
    }

    const parsed: unknown = JSON.parse(raw);
    if (!isValidDraft(parsed, investmentType)) {
      return empty;
    }

    return {
      ...empty,
      ...parsed,
      generalInputs: parseGeneralInputs(
        parsed.generalInputs,
        empty.generalInputs,
      ),
      contributions: parsed.contributions,
      contributionsConfirmed: parsed.contributionsConfirmed,
      rates: parsed.rates ?? null,
    } as SimulationDraft;
  } catch {
    return empty;
  }
}

/** Persists a simulation draft to localStorage. */
export function saveSimulationDraft(draft: SimulationDraft): void {
  try {
    localStorage.setItem(
      draftStorageKey(draft.investmentType),
      JSON.stringify(draft),
    );
  } catch {
    // Ignore quota / private-mode failures; the in-memory draft still works.
  }
}

/** Removes a simulation draft from localStorage. */
export function clearSimulationDraft(investmentType: InvestmentType): void {
  try {
    localStorage.removeItem(draftStorageKey(investmentType));
  } catch {
    // Ignore storage failures.
  }
}

export function isCdbDraft(
  draft: SimulationDraft,
): draft is CdbSimulationDraft {
  return draft.investmentType === InvestmentType.Cdb;
}

export function isTesouroDraft(
  draft: SimulationDraft,
): draft is TesouroSimulationDraft {
  return draft.investmentType === InvestmentType.TesouroSelic;
}
