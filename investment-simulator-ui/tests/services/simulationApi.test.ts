import { ApiClientError } from '@/services/ApiClientError';
import { simulateCdb, simulateTesouro } from '@/services/simulationApi';
import type { SimulationResultResponse } from '@/types/simulationApi';

const sampleResult: SimulationResultResponse = {
  startDate: '2026-01-02',
  endDate: '2026-01-09',
  initialAmount: 10000,
  totalAdditionalContributions: 0,
  totalInvested: 10000,
  grossAmount: 10100,
  grossReturnPercentage: 0.01,
  totalGrossYield: 100,
  costs: 0,
  incomeTax: 15,
  iof: 0,
  netAmount: 10085,
  netReturnPercentage: 0.0085,
  totalNetYield: 85,
  netAmountInflationAdjusted: 10085,
  contributionDetails: [],
};

describe('simulationApi', () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('posts to /simular/cdb and returns the JSON body', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      status: 200,
      headers: new Headers({ 'content-type': 'application/json' }),
      json: async () => sampleResult,
    });
    vi.stubGlobal('fetch', fetchMock);

    const result = await simulateCdb({
      initialAmount: 10000,
      startDate: '2026-01-02',
      endDate: '2026-01-09',
      contributions: [],
      cdiAnnualRates: [{ year: 2026, rate: 15 }],
      ipcaRates: [{ year: 2026, rate: 0 }],
      cdiPercentage: 1.1,
    });

    expect(result.netAmount).toBe(10085);
    expect(fetchMock).toHaveBeenCalledWith(
      '/simular/cdb',
      expect.objectContaining({ method: 'POST' }),
    );
  });

  it('posts to /simular/tesouro', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      status: 200,
      headers: new Headers({ 'content-type': 'application/json' }),
      json: async () => sampleResult,
    });
    vi.stubGlobal('fetch', fetchMock);

    await simulateTesouro({
      initialAmount: 10000,
      startDate: '2026-01-02',
      endDate: '2026-01-09',
      contributions: [],
      selicAnnualRates: [{ year: 2026, rate: 14.15 }],
      ipcaRates: [{ year: 2026, rate: 4 }],
      annualAgioRate: 0,
      b3CustodyRates: null,
    });

    expect(fetchMock).toHaveBeenCalledWith(
      '/simular/tesouro',
      expect.objectContaining({ method: 'POST' }),
    );
  });

  it('maps HTTP 400 error bodies to ApiClientError', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: false,
        status: 400,
        headers: new Headers({ 'content-type': 'application/json' }),
        json: async () => ({ error: 'Initial amount cannot be negative.' }),
      }),
    );

    await expect(
      simulateCdb({
        initialAmount: -1,
        startDate: '2026-01-02',
        endDate: '2026-01-09',
        contributions: [],
        cdiAnnualRates: [{ year: 2026, rate: 15 }],
        ipcaRates: [{ year: 2026, rate: 0 }],
        cdiPercentage: 1.1,
      }),
    ).rejects.toMatchObject({
      name: 'ApiClientError',
      message: 'Initial amount cannot be negative.',
      status: 400,
    } satisfies Partial<ApiClientError>);
  });

  it('maps network failures to a friendly ApiClientError', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockRejectedValue(new TypeError('Failed to fetch')),
    );

    await expect(
      simulateCdb({
        initialAmount: 1,
        startDate: '2026-01-02',
        endDate: '2026-01-09',
        contributions: [],
        cdiAnnualRates: [{ year: 2026, rate: 15 }],
        ipcaRates: [{ year: 2026, rate: 0 }],
        cdiPercentage: 1.1,
      }),
    ).rejects.toThrow(/conectar à api/i);
  });
});
