import { InvestmentType } from '@/types/investment';
import type {
  CdbSimulationDraft,
  TesouroSimulationDraft,
} from '@/types/simulationDraft';
import {
  mapCdbDraftToRequest,
  mapTesouroDraftToRequest,
} from '@/utils/mapSimulationRequest';

function baseCdbDraft(
  overrides: Partial<CdbSimulationDraft> = {},
): CdbSimulationDraft {
  return {
    version: 1,
    investmentType: InvestmentType.Cdb,
    generalInputs: {
      initialAmount: '10000',
      startDate: '2026-01-02',
      endDate: '2026-12-31',
    },
    contributions: [{ date: '2026-06-01', amount: '1000.50' }],
    contributionsConfirmed: true,
    rates: {
      profitabilityPercentage: '120',
      cdi: { mode: 'single', singleRate: '14.15', rates: [] },
    },
    ...overrides,
  };
}

function baseTesouroDraft(
  overrides: Partial<TesouroSimulationDraft> = {},
): TesouroSimulationDraft {
  return {
    version: 1,
    investmentType: InvestmentType.TesouroSelic,
    generalInputs: {
      initialAmount: '5000',
      startDate: '2026-01-02',
      endDate: '2027-01-02',
    },
    contributions: [],
    contributionsConfirmed: true,
    rates: {
      selic: { mode: 'single', singleRate: '14.15', rates: [] },
      annualAgioRate: '0.10',
      b3Custody: { mode: 'single', singleRate: '0.2', rates: [] },
      ipca: {
        mode: 'perYear',
        singleRate: '',
        rates: [
          { year: 2026, rate: '4.5' },
          { year: 2027, rate: '4.0' },
        ],
      },
    },
    ...overrides,
  };
}

describe('mapSimulationRequest', () => {
  it('maps a CDB draft to the API request shape', () => {
    const request = mapCdbDraftToRequest(baseCdbDraft());

    expect(request).toEqual({
      initialAmount: 10000,
      startDate: '2026-01-02',
      endDate: '2026-12-31',
      contributions: [{ date: '2026-06-01', amount: 1000.5 }],
      cdiAnnualRates: [{ year: 2026, rate: 14.15 }],
      ipcaRates: [{ year: 2026, rate: 0 }],
      cdiPercentage: 1.2,
    });
  });

  it('maps Tesouro ágio from percent to fraction and omits zero B3', () => {
    const request = mapTesouroDraftToRequest(
      baseTesouroDraft({
        rates: {
          selic: { mode: 'single', singleRate: '13', rates: [] },
          annualAgioRate: '0.10',
          b3Custody: { mode: 'single', singleRate: '0', rates: [] },
          ipca: { mode: 'single', singleRate: '4', rates: [] },
        },
      }),
    );

    expect(request.annualAgioRate).toBeCloseTo(0.001, 10);
    expect(request.b3CustodyRates).toBeNull();
    expect(request.selicAnnualRates).toEqual([{ year: 2026, rate: 13 }]);
    expect(request.ipcaRates).toEqual([{ year: 2026, rate: 4 }]);
  });

  it('maps per-year Tesouro IPCA rates', () => {
    const request = mapTesouroDraftToRequest(baseTesouroDraft());

    expect(request.ipcaRates).toEqual([
      { year: 2026, rate: 4.5 },
      { year: 2027, rate: 4 },
    ]);
    expect(request.b3CustodyRates).toEqual([{ year: 2026, rate: 0.2 }]);
  });
});
