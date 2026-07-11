/** Supported investment types (ERS sections 1–2). */
export const InvestmentType = {
  Cdb: 'Cdb',
  TesouroSelic: 'TesouroSelic',
} as const

export type InvestmentType =
  (typeof InvestmentType)[keyof typeof InvestmentType]

export const INVESTMENT_TYPE_LABELS: Record<InvestmentType, string> = {
  [InvestmentType.Cdb]: 'CDB Pós-fixado',
  [InvestmentType.TesouroSelic]: 'Tesouro Selic',
}
