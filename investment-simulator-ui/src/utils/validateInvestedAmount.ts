import type { ContributionInput } from '@/types/contribution';
import {
  isPositiveDecimalString,
  isZeroDecimalString,
} from '@/utils/parseDecimal';

/**
 * ERS §27: blocks a simulation with no money invested
 * (initial amount zero and no additional contributions).
 */
export function validateHasInvestedAmount(
  initialAmount: string,
  contributions: ContributionInput[],
): string | undefined {
  const trimmed = initialAmount.trim();
  const initialIsZeroOrEmpty =
    trimmed === '' || isZeroDecimalString(trimmed);

  if (!initialIsZeroOrEmpty) {
    return undefined;
  }

  const hasPositiveContribution = contributions.some((item) =>
    isPositiveDecimalString(item.amount),
  );

  if (hasPositiveContribution) {
    return undefined;
  }

  return 'Informe um valor inicial maior que zero ou pelo menos um aporte adicional.';
}
