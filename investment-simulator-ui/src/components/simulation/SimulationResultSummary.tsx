import type { SimulationResultResponse } from '@/types/simulationApi';
import styles from './SimulationResultSummary.module.css';

export type SimulationResultSummaryProps = {
  result: SimulationResultResponse;
  /** When false, omits the inner heading (used on the dedicated result page). */
  showHeading?: boolean;
};

function formatMoney(value: number): string {
  return value.toLocaleString('pt-BR', {
    style: 'currency',
    currency: 'BRL',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
}

function formatPercentFraction(value: number): string {
  return (value * 100).toLocaleString('pt-BR', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 4,
  });
}

/**
 * Minimal read-only summary of a simulation result (ERS §19).
 * Charts / export / contribution tables belong to later commits.
 */
export function SimulationResultSummary({
  result,
  showHeading = true,
}: SimulationResultSummaryProps) {
  const rows: Array<{ label: string; value: string }> = [
    { label: 'Valor inicial', value: formatMoney(result.initialAmount) },
    {
      label: 'Aportes adicionais',
      value: formatMoney(result.totalAdditionalContributions),
    },
    { label: 'Total investido', value: formatMoney(result.totalInvested) },
    { label: 'Valor bruto', value: formatMoney(result.grossAmount) },
    {
      label: 'Rentabilidade bruta',
      value: `${formatPercentFraction(result.grossReturnPercentage)}%`,
    },
    { label: 'Custos', value: formatMoney(result.costs) },
    { label: 'IR', value: formatMoney(result.incomeTax) },
    { label: 'IOF', value: formatMoney(result.iof) },
    { label: 'Valor líquido', value: formatMoney(result.netAmount) },
    {
      label: 'Rentabilidade líquida',
      value: `${formatPercentFraction(result.netReturnPercentage)}%`,
    },
    { label: 'Lucro líquido', value: formatMoney(result.totalNetYield) },
    {
      label: 'Valor líquido ajustado pela inflação',
      value: formatMoney(result.netAmountInflationAdjusted),
    },
  ];

  return (
    <section
      className={styles.summary}
      aria-labelledby={showHeading ? 'simulation-result-title' : undefined}
      aria-label={showHeading ? undefined : 'Resultado da simulação'}
    >
      {showHeading ? (
        <>
          <h2 id="simulation-result-title" className={styles.title}>
            Resultado da simulação
          </h2>
          <p className={styles.hint}>
            Resumo final. Gráficos e exportação serão adicionados em etapas
            seguintes.
          </p>
        </>
      ) : (
        <p className={styles.hint}>
          Resumo final. Gráficos e exportação serão adicionados em etapas
          seguintes.
        </p>
      )}
      <dl className={styles.list}>
        {rows.map((row) => (
          <div key={row.label} className={styles.row}>
            <dt>{row.label}</dt>
            <dd>{row.value}</dd>
          </div>
        ))}
      </dl>
    </section>
  );
}
