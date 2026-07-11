import type {
  GeneralInputs,
  GeneralInputsErrors,
} from '@/types/generalInputs'
import { InvestmentType } from '@/types/investment'
import { isValidIsoDate } from '@/utils/isoDate'

/** Matches a non-negative decimal with optional fractional part. */
const DECIMAL_PATTERN = /^(?:0|[1-9]\d*)(?:\.\d+)?$/

/**
 * Validates general inputs (ERS sections 3 and 27) for the fields
 * in scope of this form: amount, dates and investment type.
 */
export function validateGeneralInputs(
  values: GeneralInputs,
): GeneralInputsErrors {
  const errors: GeneralInputsErrors = {}

  if (
    values.investmentType !== InvestmentType.Cdb &&
    values.investmentType !== InvestmentType.TesouroSelic
  ) {
    errors.investmentType = 'Selecione o tipo de investimento.'
  }

  const amount = values.initialAmount.trim()
  if (amount === '') {
    errors.initialAmount = 'Informe o valor inicial (zero é permitido).'
  } else if (!DECIMAL_PATTERN.test(amount)) {
    errors.initialAmount =
      'Informe um valor monetário válido (não negativo).'
  }

  if (!values.startDate) {
    errors.startDate = 'Informe a data inicial.'
  } else if (!isValidIsoDate(values.startDate)) {
    errors.startDate = 'Data inicial inválida.'
  }

  if (!values.endDate) {
    errors.endDate = 'Informe a data de resgate.'
  } else if (!isValidIsoDate(values.endDate)) {
    errors.endDate = 'Data de resgate inválida.'
  }

  if (
    !errors.startDate &&
    !errors.endDate &&
    values.endDate < values.startDate
  ) {
    errors.endDate =
      'A data de resgate não pode ser anterior à data inicial.'
  }

  return errors
}

export function hasGeneralInputsErrors(
  errors: GeneralInputsErrors,
): boolean {
  return Object.keys(errors).length > 0
}
