import { InvestmentType } from '@/types/investment';
import type { GeneralInputs } from '@/types/generalInputs';
import {
  hasGeneralInputsErrors,
  validateGeneralInputs,
} from '@/utils/validateGeneralInputs';

function validInputs(overrides: Partial<GeneralInputs> = {}): GeneralInputs {
  return {
    investmentType: InvestmentType.Cdb,
    initialAmount: '10000',
    startDate: '2026-01-01',
    endDate: '2027-01-01',
    ...overrides,
  };
}

describe('validateGeneralInputs', () => {
  it('accepts valid general inputs including zero initial amount', () => {
    const errors = validateGeneralInputs(validInputs({ initialAmount: '0' }));

    expect(errors).toEqual({});
    expect(hasGeneralInputsErrors(errors)).toBe(false);
  });

  it('rejects negative or malformed initial amounts', () => {
    expect(
      validateGeneralInputs(validInputs({ initialAmount: '-1' })).initialAmount,
    ).toBeDefined();

    expect(
      validateGeneralInputs(validInputs({ initialAmount: '10,50' }))
        .initialAmount,
    ).toBeDefined();

    expect(
      validateGeneralInputs(validInputs({ initialAmount: '' })).initialAmount,
    ).toBeDefined();
  });

  it('rejects redemption date before start date', () => {
    const errors = validateGeneralInputs(
      validInputs({
        startDate: '2026-06-01',
        endDate: '2026-05-01',
      }),
    );

    expect(errors.endDate).toMatch(/não pode ser anterior/i);
  });

  it('rejects missing dates', () => {
    const errors = validateGeneralInputs(
      validInputs({ startDate: '', endDate: '' }),
    );

    expect(errors.startDate).toBeDefined();
    expect(errors.endDate).toBeDefined();
  });
});
