import { validateHasInvestedAmount } from '@/utils/validateInvestedAmount';

describe('validateHasInvestedAmount', () => {
  it('allows positive initial amount with no contributions', () => {
    expect(validateHasInvestedAmount('1000', [])).toBeUndefined();
  });

  it('allows zero initial amount when there are positive contributions', () => {
    expect(
      validateHasInvestedAmount('0', [{ date: '2026-01-01', amount: '500' }]),
    ).toBeUndefined();
  });

  it('rejects zero initial amount without contributions', () => {
    expect(validateHasInvestedAmount('0', [])).toMatch(/aporte adicional/i);
    expect(validateHasInvestedAmount('0.00', [])).toMatch(/aporte adicional/i);
  });
});
