/**
 * API contracts for POST /simular/cdb and POST /simular/tesouro.
 * Monetary and rate values are serialized as JSON numbers (ASP.NET `decimal`).
 * UI forms keep string decimals; mappers convert at the boundary.
 */

/** Additional contribution in an API request body. */
export type ContributionRequest = {
  date: string;
  amount: number;
};

/** Annual rate for one calendar year (API percentage, e.g. 14.15 = 14.15% a.a.). */
export type AnnualRateRequest = {
  year: number;
  rate: number;
};

/** Body for POST /simular/cdb. */
export type SimulateCdbRequest = {
  initialAmount: number;
  startDate: string;
  endDate: string;
  contributions: ContributionRequest[];
  cdiAnnualRates: AnnualRateRequest[];
  ipcaRates: AnnualRateRequest[];
  /** CDI multiplier as a fraction (e.g. 1.2 = 120% of CDI). */
  cdiPercentage: number;
};

/** Body for POST /simular/tesouro. */
export type SimulateTesouroRequest = {
  initialAmount: number;
  startDate: string;
  endDate: string;
  contributions: ContributionRequest[];
  selicAnnualRates: AnnualRateRequest[];
  ipcaRates: AnnualRateRequest[];
  /** Ágio/deságio as a decimal fraction (e.g. 0.001 = +0.1% a.a.). */
  annualAgioRate: number;
  b3CustodyRates?: AnnualRateRequest[] | null;
};

/** Per-contribution breakdown in the simulation result. */
export type ContributionDetailResponse = {
  date: string;
  amount: number;
  grossBalance: number;
  grossYield: number;
  calendarDaysInvested: number;
  businessDaysInvested: number;
  incomeTax: number;
  iof: number;
};

/** Summary returned by simulation endpoints (HTTP 200). */
export type SimulationResultResponse = {
  /** Simulation start date (YYYY-MM-DD). */
  startDate: string;
  /** Redemption / end date (YYYY-MM-DD). */
  endDate: string;
  initialAmount: number;
  totalAdditionalContributions: number;
  totalInvested: number;
  grossAmount: number;
  grossReturnPercentage: number;
  /** Gross profit (gross amount − total invested). */
  totalGrossYield: number;
  costs: number;
  incomeTax: number;
  iof: number;
  netAmount: number;
  netReturnPercentage: number;
  totalNetYield: number;
  netAmountInflationAdjusted: number;
  contributionDetails: ContributionDetailResponse[];
};

/** Shape of HTTP 400 bodies from DomainExceptionHandler. */
export type ApiErrorBody = {
  error?: string;
};
