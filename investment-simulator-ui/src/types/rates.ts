/**
 * Rate registration types from ERS section 6.
 * Percentages are kept as strings to avoid float precision loss.
 */

/** How annual rates are supplied for a simulation period. */
export type RateEntryMode = 'single' | 'perYear';

/** Annual rate for one calendar year (percentage string, e.g. "15" = 15% a.a.). */
export type AnnualRateInput = {
  year: number;
  /** Annual rate as a percentage string (e.g. "14.15"). */
  rate: string;
};

/**
 * Schedule for one rate series: either a single rate for every year
 * or one rate per calendar year in the period.
 */
export type RateScheduleInput = {
  mode: RateEntryMode;
  /** Used when mode is `single`. */
  singleRate: string;
  /** Used when mode is `perYear` (one entry per year). */
  rates: AnnualRateInput[];
};

/** CDB rates: profitability (% of CDI) and CDI annual schedule. */
export type CdbRatesInput = {
  /** Profitability as % of CDI (e.g. "120" = 120% of CDI). */
  profitabilityPercentage: string;
  cdi: RateScheduleInput;
};

/** Tesouro Selic rates: Selic Over, ágio/deságio, B3 custody and IPCA. */
export type TesouroRatesInput = {
  selic: RateScheduleInput;
  agio: RateScheduleInput;
  b3Custody: RateScheduleInput;
  ipca: RateScheduleInput;
};

export type RateScheduleErrors = {
  mode?: string;
  singleRate?: string;
  rates?: Record<number, string>;
};

export type CdbRatesErrors = {
  profitabilityPercentage?: string;
  cdi?: RateScheduleErrors;
};

export type TesouroRatesErrors = {
  selic?: RateScheduleErrors;
  agio?: RateScheduleErrors;
  b3Custody?: RateScheduleErrors;
  ipca?: RateScheduleErrors;
};
