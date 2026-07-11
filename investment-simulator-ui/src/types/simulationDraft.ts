import type { ContributionInput } from '@/types/contribution';
import type { GeneralInputs } from '@/types/generalInputs';
import { InvestmentType } from '@/types/investment';
import type { CdbRatesInput, TesouroRatesInput } from '@/types/rates';

/** Shared fields for an in-progress simulation draft (persisted locally). */
export type SimulationDraftBase = {
  version: 1;
  generalInputs: Pick<GeneralInputs, 'initialAmount' | 'startDate' | 'endDate'>;
  contributions: ContributionInput[];
  /** True after the contributions step was successfully submitted. */
  contributionsConfirmed: boolean;
};

export type CdbSimulationDraft = SimulationDraftBase & {
  investmentType: typeof InvestmentType.Cdb;
  rates: CdbRatesInput | null;
};

export type TesouroSimulationDraft = SimulationDraftBase & {
  investmentType: typeof InvestmentType.TesouroSelic;
  rates: TesouroRatesInput | null;
};

export type SimulationDraft = CdbSimulationDraft | TesouroSimulationDraft;

export function createEmptyCdbDraft(): CdbSimulationDraft {
  return {
    version: 1,
    investmentType: InvestmentType.Cdb,
    generalInputs: {
      initialAmount: '0',
      startDate: '',
      endDate: '',
    },
    contributions: [],
    contributionsConfirmed: false,
    rates: null,
  };
}

export function createEmptyTesouroDraft(): TesouroSimulationDraft {
  return {
    version: 1,
    investmentType: InvestmentType.TesouroSelic,
    generalInputs: {
      initialAmount: '0',
      startDate: '',
      endDate: '',
    },
    contributions: [],
    contributionsConfirmed: false,
    rates: null,
  };
}

export function createEmptyDraft(
  investmentType: InvestmentType,
): SimulationDraft {
  return investmentType === InvestmentType.Cdb
    ? createEmptyCdbDraft()
    : createEmptyTesouroDraft();
}
