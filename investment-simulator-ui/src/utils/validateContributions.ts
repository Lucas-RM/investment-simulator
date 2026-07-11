import type {
  ContributionInput,
  ContributionRowErrors,
  ContributionsErrors,
} from '@/types/contribution';
import { isValidIsoDate } from '@/utils/isoDate';

/** Matches a non-negative decimal with optional fractional part. */
const DECIMAL_PATTERN = /^(?:0|[1-9]\d*)(?:\.\d+)?$/;

export type ValidateContributionsContext = {
  /** First contribution / simulation start date (YYYY-MM-DD). */
  startDate: string;
  /** Redemption (end) date (YYYY-MM-DD). */
  endDate: string;
};

function isPositiveDecimal(value: string): boolean {
  if (!DECIMAL_PATTERN.test(value)) {
    return false;
  }

  return !/^0+(?:\.0+)?$/.test(value);
}

/**
 * Validates a single contribution row (ERS sections 4 and 5).
 * Rules: amount > 0, valid date, startDate ≤ date ≤ endDate.
 */
export function validateContributionRow(
  values: ContributionInput,
  context: ValidateContributionsContext,
): ContributionRowErrors {
  const errors: ContributionRowErrors = {};

  const amount = values.amount.trim();
  if (amount === '') {
    errors.amount = 'Informe o valor do aporte.';
  } else if (!isPositiveDecimal(amount)) {
    errors.amount = 'O valor do aporte deve ser maior que zero.';
  }

  if (!values.date) {
    errors.date = 'Informe a data do aporte.';
  } else if (!isValidIsoDate(values.date)) {
    errors.date = 'Data do aporte inválida.';
  } else {
    if (
      context.startDate &&
      isValidIsoDate(context.startDate) &&
      values.date < context.startDate
    ) {
      errors.date =
        'A data do aporte não pode ser anterior à data do primeiro aporte.';
    }

    if (
      context.endDate &&
      isValidIsoDate(context.endDate) &&
      values.date > context.endDate
    ) {
      errors.date =
        'A data do aporte não pode ser posterior à data de resgate.';
    }
  }

  return errors;
}

/**
 * Validates all contribution rows keyed by row id (ERS sections 4 and 5).
 */
export function validateContributions(
  rows: Array<ContributionInput & { id: string }>,
  context: ValidateContributionsContext,
): ContributionsErrors {
  const errors: ContributionsErrors = {};

  for (const row of rows) {
    const rowErrors = validateContributionRow(row, context);
    if (Object.keys(rowErrors).length > 0) {
      errors[row.id] = rowErrors;
    }
  }

  return errors;
}

export function hasContributionsErrors(errors: ContributionsErrors): boolean {
  return Object.keys(errors).length > 0;
}
