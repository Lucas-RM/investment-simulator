import type { InvestmentType } from '@/types/investment'

/**
 * General simulation inputs from ERS section 3
 * (initial amount, dates, investment type).
 * Monetary values are kept as strings to avoid float precision loss.
 */
export type GeneralInputs = {
  investmentType: InvestmentType
  /** Initial amount in BRL as a decimal string (e.g. "10000.50"). */
  initialAmount: string
  /** Simulation start / initial contribution date (YYYY-MM-DD). */
  startDate: string
  /** Redemption (end) date (YYYY-MM-DD). */
  endDate: string
}

export type GeneralInputsErrors = Partial<Record<keyof GeneralInputs, string>>
