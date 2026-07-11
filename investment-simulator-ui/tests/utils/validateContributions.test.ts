import type { ContributionInput } from '@/types/contribution';
import {
  hasContributionsErrors,
  validateContributionRow,
  validateContributions,
} from '@/utils/validateContributions';

const context = {
  startDate: '2026-01-01',
  endDate: '2027-01-01',
};

function validContribution(
  overrides: Partial<ContributionInput> = {},
): ContributionInput {
  return {
    date: '2026-06-01',
    amount: '1000',
    ...overrides,
  };
}

describe('validateContributions', () => {
  it('accepts valid contributions including an empty list', () => {
    expect(validateContributions([], context)).toEqual({});
    expect(validateContributionRow(validContribution(), context)).toEqual({});
    expect(hasContributionsErrors({})).toBe(false);
  });

  it('rejects missing or non-positive amounts', () => {
    expect(
      validateContributionRow(validContribution({ amount: '' }), context)
        .amount,
    ).toMatch(/informe o valor/i);

    expect(
      validateContributionRow(validContribution({ amount: '0' }), context)
        .amount,
    ).toMatch(/maior que zero/i);

    expect(
      validateContributionRow(validContribution({ amount: '0.00' }), context)
        .amount,
    ).toMatch(/maior que zero/i);

    expect(
      validateContributionRow(validContribution({ amount: '-10' }), context)
        .amount,
    ).toMatch(/maior que zero/i);
  });

  it('rejects missing or invalid dates', () => {
    expect(
      validateContributionRow(validContribution({ date: '' }), context).date,
    ).toMatch(/informe a data/i);

    expect(
      validateContributionRow(
        validContribution({ date: '2026-02-30' }),
        context,
      ).date,
    ).toMatch(/inválida/i);
  });

  it('rejects dates before the first contribution date', () => {
    const errors = validateContributionRow(
      validContribution({ date: '2025-12-31' }),
      context,
    );

    expect(errors.date).toMatch(/anterior à data do primeiro aporte/i);
  });

  it('rejects dates after the redemption date', () => {
    const errors = validateContributionRow(
      validContribution({ date: '2027-01-02' }),
      context,
    );

    expect(errors.date).toMatch(/posterior à data de resgate/i);
  });

  it('accepts dates on the period boundaries', () => {
    expect(
      validateContributionRow(
        validContribution({ date: '2026-01-01' }),
        context,
      ),
    ).toEqual({});

    expect(
      validateContributionRow(
        validContribution({ date: '2027-01-01' }),
        context,
      ),
    ).toEqual({});
  });

  it('maps row errors by id', () => {
    const errors = validateContributions(
      [
        { id: 'a', date: '2026-02-01', amount: '500' },
        { id: 'b', date: '', amount: '0' },
      ],
      context,
    );

    expect(errors.a).toBeUndefined();
    expect(errors.b?.date).toBeDefined();
    expect(errors.b?.amount).toBeDefined();
    expect(hasContributionsErrors(errors)).toBe(true);
  });
});
