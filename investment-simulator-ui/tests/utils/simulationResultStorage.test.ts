import {
  clearSimulationResult,
  loadSimulationResult,
  resultStorageKey,
  saveSimulationResult,
} from '@/utils/simulationResultStorage';
import { InvestmentType } from '@/types/investment';
import type { SimulationResultResponse } from '@/types/simulationApi';

const sampleResult: SimulationResultResponse = {
  initialAmount: 10000,
  totalAdditionalContributions: 1000,
  totalInvested: 11000,
  grossAmount: 12000,
  grossReturnPercentage: 0.09,
  costs: 0,
  incomeTax: 150,
  iof: 0,
  netAmount: 11850,
  netReturnPercentage: 0.077,
  totalNetYield: 850,
  netAmountInflationAdjusted: 11500,
  contributionDetails: [],
};

describe('simulationResultStorage', () => {
  beforeEach(() => {
    sessionStorage.clear();
  });

  it('persists and reloads a result', () => {
    saveSimulationResult(InvestmentType.Cdb, sampleResult);

    expect(loadSimulationResult(InvestmentType.Cdb)).toEqual(sampleResult);
    expect(
      sessionStorage.getItem(resultStorageKey(InvestmentType.Cdb)),
    ).toContain('11850');
  });

  it('returns null when nothing is stored', () => {
    expect(loadSimulationResult(InvestmentType.Cdb)).toBeNull();
  });

  it('clears a stored result', () => {
    saveSimulationResult(InvestmentType.Cdb, sampleResult);
    clearSimulationResult(InvestmentType.Cdb);

    expect(loadSimulationResult(InvestmentType.Cdb)).toBeNull();
  });

  it('ignores corrupted payloads', () => {
    sessionStorage.setItem(
      resultStorageKey(InvestmentType.TesouroSelic),
      '{not-json',
    );

    expect(loadSimulationResult(InvestmentType.TesouroSelic)).toBeNull();
  });
});
