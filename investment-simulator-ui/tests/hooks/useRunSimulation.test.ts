import { renderHook, act, waitFor } from '@testing-library/react';
import { useRunSimulation } from '@/hooks/useRunSimulation';
import { InvestmentType } from '@/types/investment';
import type { CdbSimulationDraft } from '@/types/simulationDraft';
import type { SimulationResultResponse } from '@/types/simulationApi';

const sampleResult: SimulationResultResponse = {
  initialAmount: 10000,
  totalAdditionalContributions: 0,
  totalInvested: 10000,
  grossAmount: 10100,
  grossReturnPercentage: 0.01,
  costs: 0,
  incomeTax: 15,
  iof: 0,
  netAmount: 10085,
  netReturnPercentage: 0.0085,
  totalNetYield: 85,
  netAmountInflationAdjusted: 10085,
  contributionDetails: [],
};

const draft: CdbSimulationDraft = {
  version: 1,
  investmentType: InvestmentType.Cdb,
  generalInputs: {
    initialAmount: '10000',
    startDate: '2026-01-02',
    endDate: '2026-01-09',
  },
  contributions: [],
  contributionsConfirmed: true,
  rates: {
    profitabilityPercentage: '110',
    cdi: { mode: 'single', singleRate: '15', rates: [] },
  },
};

describe('useRunSimulation', () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('exposes loading then success with the API result', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        status: 200,
        headers: new Headers({ 'content-type': 'application/json' }),
        json: async () => sampleResult,
      }),
    );

    const { result } = renderHook(() => useRunSimulation());

    expect(result.current.status).toBe('idle');

    await act(async () => {
      await result.current.run(draft);
    });

    await waitFor(() => {
      expect(result.current.status).toBe('success');
    });

    expect(result.current.result?.netAmount).toBe(10085);
    expect(result.current.error).toBeNull();
    expect(result.current.isLoading).toBe(false);
  });

  it('exposes error state when the API returns 400', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: false,
        status: 400,
        headers: new Headers({ 'content-type': 'application/json' }),
        json: async () => ({
          error:
            'When initial amount is zero, at least one additional contribution is required.',
        }),
      }),
    );

    const { result } = renderHook(() => useRunSimulation());

    await act(async () => {
      await result.current.run(draft);
    });

    expect(result.current.status).toBe('error');
    expect(result.current.error).toMatch(/initial amount is zero/i);
    expect(result.current.result).toBeNull();
  });
});
