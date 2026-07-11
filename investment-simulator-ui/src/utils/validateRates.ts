import type {
  CdbRatesErrors,
  CdbRatesInput,
  RateScheduleErrors,
  RateScheduleInput,
  TesouroRatesErrors,
  TesouroRatesInput,
} from '@/types/rates';
import { generateYears } from '@/utils/generateYears';
import { isValidIsoDate } from '@/utils/isoDate';

/** Non-negative decimal with optional fractional part. */
const NON_NEGATIVE_DECIMAL = /^(?:0|[1-9]\d*)(?:\.\d+)?$/;

/** Signed decimal (allows leading minus for ágio/deságio). */
const SIGNED_DECIMAL = /^-?(?:0|[1-9]\d*)(?:\.\d+)?$/;

export type ValidateRatesContext = {
  startDate: string;
  endDate: string;
};

export type ValidateRateScheduleOptions = {
  /** Field label used in error messages (e.g. "CDI"). */
  label: string;
  /** When true, negative percentages are allowed (ágio/deságio). */
  allowNegative?: boolean;
  /**
   * Exclusive lower bound as a percentage (e.g. -100 → rate must be > -100).
   * Only applied when allowNegative is true.
   */
  minExclusivePercent?: number;
};

function parseDecimal(value: string): number | null {
  if (!SIGNED_DECIMAL.test(value)) {
    return null;
  }
  return Number(value);
}

function validatePercentageValue(
  raw: string,
  label: string,
  options: Pick<
    ValidateRateScheduleOptions,
    'allowNegative' | 'minExclusivePercent'
  >,
): string | undefined {
  const value = raw.trim();

  if (value === '') {
    return `Informe a taxa de ${label}.`;
  }

  if (options.allowNegative) {
    if (!SIGNED_DECIMAL.test(value)) {
      return `Informe uma taxa válida para ${label}.`;
    }

    const numeric = parseDecimal(value);
    if (numeric === null) {
      return `Informe uma taxa válida para ${label}.`;
    }

    const minExclusive = options.minExclusivePercent ?? -100;
    if (numeric <= minExclusive) {
      return `A taxa de ${label} deve ser maior que ${minExclusive}%.`;
    }

    return undefined;
  }

  if (!NON_NEGATIVE_DECIMAL.test(value)) {
    return `A taxa de ${label} não pode ser negativa.`;
  }

  return undefined;
}

/**
 * Validates a rate schedule (single or per-year) for ERS sections 6 and 27.
 */
export function validateRateSchedule(
  schedule: RateScheduleInput,
  context: ValidateRatesContext,
  options: ValidateRateScheduleOptions,
): RateScheduleErrors {
  const errors: RateScheduleErrors = {};

  if (schedule.mode !== 'single' && schedule.mode !== 'perYear') {
    errors.mode = 'Selecione o modo de entrada da taxa.';
    return errors;
  }

  if (schedule.mode === 'single') {
    const singleError = validatePercentageValue(
      schedule.singleRate,
      options.label,
      options,
    );
    if (singleError) {
      errors.singleRate = singleError;
    }
    return errors;
  }

  const expectedYears =
    isValidIsoDate(context.startDate) && isValidIsoDate(context.endDate)
      ? generateYears(context.startDate, context.endDate)
      : [];

  if (expectedYears.length === 0) {
    errors.mode =
      'Informe datas válidas na etapa anterior para preencher as taxas ano a ano.';
    return errors;
  }

  const rateErrors: Record<number, string> = {};
  const seenYears = new Set<number>();

  for (const entry of schedule.rates) {
    if (seenYears.has(entry.year)) {
      rateErrors[entry.year] = `Taxa duplicada para o ano ${entry.year}.`;
      continue;
    }
    seenYears.add(entry.year);

    const entryError = validatePercentageValue(
      entry.rate,
      options.label,
      options,
    );
    if (entryError) {
      rateErrors[entry.year] = entryError;
    }
  }

  for (const year of expectedYears) {
    if (!seenYears.has(year)) {
      rateErrors[year] = `Informe a taxa de ${options.label} para ${year}.`;
    }
  }

  for (const year of seenYears) {
    if (!expectedYears.includes(year) && !rateErrors[year]) {
      rateErrors[year] = `O ano ${year} está fora do período da simulação.`;
    }
  }

  if (Object.keys(rateErrors).length > 0) {
    errors.rates = rateErrors;
  }

  return errors;
}

function hasScheduleErrors(errors: RateScheduleErrors | undefined): boolean {
  if (!errors) {
    return false;
  }

  return Boolean(
    errors.mode ||
    errors.singleRate ||
    (errors.rates && Object.keys(errors.rates).length > 0),
  );
}

/**
 * Validates CDB rates: profitability (% of CDI) and CDI schedule (ERS section 6).
 */
export function validateCdbRates(
  values: CdbRatesInput,
  context: ValidateRatesContext,
): CdbRatesErrors {
  const errors: CdbRatesErrors = {};

  const profitability = values.profitabilityPercentage.trim();
  if (profitability === '') {
    errors.profitabilityPercentage = 'Informe a rentabilidade do CDB.';
  } else if (!NON_NEGATIVE_DECIMAL.test(profitability)) {
    errors.profitabilityPercentage =
      'Informe uma rentabilidade válida (não negativa).';
  } else if (/^0+(?:\.0+)?$/.test(profitability)) {
    errors.profitabilityPercentage = 'A rentabilidade deve ser maior que zero.';
  }

  const cdiErrors = validateRateSchedule(values.cdi, context, { label: 'CDI' });
  if (hasScheduleErrors(cdiErrors)) {
    errors.cdi = cdiErrors;
  }

  return errors;
}

/**
 * Validates the single annual ágio/deságio percentage (API `AnnualAgioRate`).
 * Must be greater than -100% so the equivalent fraction stays above -1.
 */
export function validateAnnualAgioRate(raw: string): string | undefined {
  return validatePercentageValue(raw, 'ágio/deságio', {
    allowNegative: true,
    minExclusivePercent: -100,
  });
}

/**
 * Validates Tesouro Selic rates: Selic, ágio/deságio, B3 and IPCA (ERS section 6).
 */
export function validateTesouroRates(
  values: TesouroRatesInput,
  context: ValidateRatesContext,
): TesouroRatesErrors {
  const errors: TesouroRatesErrors = {};

  const selicErrors = validateRateSchedule(values.selic, context, {
    label: 'Selic Over',
  });
  if (hasScheduleErrors(selicErrors)) {
    errors.selic = selicErrors;
  }

  const agioError = validateAnnualAgioRate(values.annualAgioRate);
  if (agioError) {
    errors.annualAgioRate = agioError;
  }

  const b3Errors = validateRateSchedule(values.b3Custody, context, {
    label: 'custódia B3',
  });
  if (hasScheduleErrors(b3Errors)) {
    errors.b3Custody = b3Errors;
  }

  const ipcaErrors = validateRateSchedule(values.ipca, context, {
    label: 'IPCA',
  });
  if (hasScheduleErrors(ipcaErrors)) {
    errors.ipca = ipcaErrors;
  }

  return errors;
}

export function hasCdbRatesErrors(errors: CdbRatesErrors): boolean {
  return Boolean(
    errors.profitabilityPercentage || hasScheduleErrors(errors.cdi),
  );
}

export function hasTesouroRatesErrors(errors: TesouroRatesErrors): boolean {
  return Boolean(
    hasScheduleErrors(errors.selic) ||
    errors.annualAgioRate ||
    hasScheduleErrors(errors.b3Custody) ||
    hasScheduleErrors(errors.ipca),
  );
}
