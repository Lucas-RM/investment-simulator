import type { CdbRatesInput, TesouroRatesInput } from '@/types/rates';
import { createEmptyRateSchedule } from '@/utils/rateSchedule';
import {
  hasCdbRatesErrors,
  hasTesouroRatesErrors,
  validateCdbRates,
  validateRateSchedule,
  validateTesouroRates,
} from '@/utils/validateRates';

const context = {
  startDate: '2026-01-01',
  endDate: '2028-06-01',
};

function validCdbRates(overrides: Partial<CdbRatesInput> = {}): CdbRatesInput {
  return {
    profitabilityPercentage: '120',
    cdi: {
      mode: 'single',
      singleRate: '15',
      rates: [],
    },
    ...overrides,
  };
}

function validTesouroRates(
  overrides: Partial<TesouroRatesInput> = {},
): TesouroRatesInput {
  return {
    selic: {
      mode: 'single',
      singleRate: '14.15',
      rates: [],
    },
    agio: {
      mode: 'single',
      singleRate: '0.1',
      rates: [],
    },
    b3Custody: {
      mode: 'single',
      singleRate: '0.2',
      rates: [],
    },
    ipca: {
      mode: 'single',
      singleRate: '4.5',
      rates: [],
    },
    ...overrides,
  };
}

describe('validateRateSchedule', () => {
  it('accepts a valid single rate', () => {
    expect(
      validateRateSchedule(
        { mode: 'single', singleRate: '15', rates: [] },
        context,
        { label: 'CDI' },
      ),
    ).toEqual({});
  });

  it('rejects empty or negative single rates', () => {
    expect(
      validateRateSchedule(
        { mode: 'single', singleRate: '', rates: [] },
        context,
        { label: 'CDI' },
      ).singleRate,
    ).toMatch(/informe a taxa/i);

    expect(
      validateRateSchedule(
        { mode: 'single', singleRate: '-1', rates: [] },
        context,
        { label: 'CDI' },
      ).singleRate,
    ).toMatch(/não pode ser negativa/i);
  });

  it('allows negative ágio/deságio above -100%', () => {
    expect(
      validateRateSchedule(
        { mode: 'single', singleRate: '-0.1', rates: [] },
        context,
        { label: 'ágio/deságio', allowNegative: true },
      ),
    ).toEqual({});

    expect(
      validateRateSchedule(
        { mode: 'single', singleRate: '-100', rates: [] },
        context,
        { label: 'ágio/deságio', allowNegative: true },
      ).singleRate,
    ).toMatch(/maior que -100/i);
  });

  it('requires one rate per year in per-year mode', () => {
    const errors = validateRateSchedule(
      {
        mode: 'perYear',
        singleRate: '',
        rates: [
          { year: 2026, rate: '15' },
          { year: 2027, rate: '' },
          { year: 2028, rate: '11' },
        ],
      },
      context,
      { label: 'CDI' },
    );

    expect(errors.rates?.[2027]).toMatch(/informe a taxa/i);
    expect(errors.rates?.[2026]).toBeUndefined();
  });
});

describe('validateCdbRates', () => {
  it('accepts valid CDB rates', () => {
    expect(validateCdbRates(validCdbRates(), context)).toEqual({});
    expect(hasCdbRatesErrors({})).toBe(false);
  });

  it('rejects missing or zero profitability', () => {
    expect(
      validateCdbRates(validCdbRates({ profitabilityPercentage: '' }), context)
        .profitabilityPercentage,
    ).toMatch(/informe a rentabilidade/i);

    expect(
      validateCdbRates(validCdbRates({ profitabilityPercentage: '0' }), context)
        .profitabilityPercentage,
    ).toMatch(/maior que zero/i);
  });

  it('rejects invalid CDI schedule', () => {
    const errors = validateCdbRates(
      validCdbRates({ cdi: createEmptyRateSchedule() }),
      context,
    );

    expect(errors.cdi?.singleRate).toBeDefined();
    expect(hasCdbRatesErrors(errors)).toBe(true);
  });
});

describe('validateTesouroRates', () => {
  it('accepts valid Tesouro rates', () => {
    expect(validateTesouroRates(validTesouroRates(), context)).toEqual({});
    expect(hasTesouroRatesErrors({})).toBe(false);
  });

  it('collects errors for each schedule', () => {
    const errors = validateTesouroRates(
      {
        selic: createEmptyRateSchedule(),
        agio: createEmptyRateSchedule(),
        b3Custody: createEmptyRateSchedule(),
        ipca: createEmptyRateSchedule(),
      },
      context,
    );

    expect(errors.selic?.singleRate).toBeDefined();
    expect(errors.agio?.singleRate).toBeDefined();
    expect(errors.b3Custody?.singleRate).toBeDefined();
    expect(errors.ipca?.singleRate).toBeDefined();
    expect(hasTesouroRatesErrors(errors)).toBe(true);
  });
});
