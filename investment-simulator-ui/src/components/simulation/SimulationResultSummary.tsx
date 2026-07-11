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
  return `${(value * 100).toLocaleString('pt-BR', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  })}%`;
}

function formatDate(isoDate: string): string {
  const [year, month, day] = isoDate.split('-').map(Number);
  if (!year || !month || !day) {
    return isoDate;
  }

  return new Date(Date.UTC(year, month - 1, day)).toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    timeZone: 'UTC',
  });
}

function signedClass(value: number): string {
  if (value > 0) {
    return styles.positive;
  }
  if (value < 0) {
    return styles.negative;
  }
  return '';
}

type DetailRow = {
  label: string;
  value: string;
  tone?: 'positive' | 'negative' | 'muted';
};

/**
 * Simulation result summary (ERS §19) with highlighted metrics and grouped details.
 */
export function SimulationResultSummary({
  result,
  showHeading = true,
}: SimulationResultSummaryProps) {
  const investmentRows: DetailRow[] = [
    { label: 'Valor inicial', value: formatMoney(result.initialAmount) },
    {
      label: 'Aportes adicionais',
      value: formatMoney(result.totalAdditionalContributions),
    },
    { label: 'Total investido', value: formatMoney(result.totalInvested) },
  ];

  const performanceRows: DetailRow[] = [
    { label: 'Valor bruto', value: formatMoney(result.grossAmount) },
    {
      label: 'Lucro bruto',
      value: formatMoney(result.totalGrossYield),
      tone: result.totalGrossYield >= 0 ? 'positive' : 'negative',
    },
    {
      label: 'Rentabilidade bruta',
      value: formatPercentFraction(result.grossReturnPercentage),
      tone: result.grossReturnPercentage >= 0 ? 'positive' : 'negative',
    },
  ];

  const deductionRows: DetailRow[] = [
    { label: 'Custos', value: formatMoney(result.costs), tone: 'muted' },
    { label: 'IR', value: formatMoney(result.incomeTax), tone: 'muted' },
    { label: 'IOF', value: formatMoney(result.iof), tone: 'muted' },
  ];

  const finalRows: DetailRow[] = [
    { label: 'Valor líquido', value: formatMoney(result.netAmount) },
    {
      label: 'Lucro líquido',
      value: formatMoney(result.totalNetYield),
      tone: result.totalNetYield >= 0 ? 'positive' : 'negative',
    },
    {
      label: 'Rentabilidade líquida',
      value: formatPercentFraction(result.netReturnPercentage),
      tone: result.netReturnPercentage >= 0 ? 'positive' : 'negative',
    },
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
        <h2 id="simulation-result-title" className={styles.title}>
          Resultado da simulação
        </h2>
      ) : null}

      <div className={styles.period} aria-label="Período da simulação">
        <div className={styles.periodItem}>
          <span className={styles.periodLabel}>Data inicial</span>
          <strong className={styles.periodValue}>
            {formatDate(result.startDate)}
          </strong>
        </div>
        <span className={styles.periodArrow} aria-hidden="true">
          →
        </span>
        <div className={styles.periodItem}>
          <span className={styles.periodLabel}>Data de resgate</span>
          <strong className={styles.periodValue}>
            {formatDate(result.endDate)}
          </strong>
        </div>
      </div>

      <div className={styles.highlights} aria-label="Indicadores principais">
        <article className={`${styles.highlightCard} ${styles.highlightPrimary}`}>
          <p className={styles.highlightLabel}>Valor líquido</p>
          <p className={styles.highlightValue}>
            {formatMoney(result.netAmount)}
          </p>
        </article>
        <article className={styles.highlightCard}>
          <p className={styles.highlightLabel}>Lucro líquido</p>
          <p
            className={`${styles.highlightValue} ${signedClass(result.totalNetYield)}`}
          >
            {formatMoney(result.totalNetYield)}
          </p>
        </article>
        <article className={styles.highlightCard}>
          <p className={styles.highlightLabel}>Rentabilidade líquida</p>
          <p
            className={`${styles.highlightValue} ${signedClass(result.netReturnPercentage)}`}
          >
            {formatPercentFraction(result.netReturnPercentage)}
          </p>
        </article>
        <article className={styles.highlightCard}>
          <p className={styles.highlightLabel}>Lucro bruto</p>
          <p
            className={`${styles.highlightValue} ${signedClass(result.totalGrossYield)}`}
          >
            {formatMoney(result.totalGrossYield)}
          </p>
        </article>
      </div>

      <div className={styles.groups}>
        <DetailGroup title="Investimento" rows={investmentRows} />
        <DetailGroup title="Rentabilidade bruta" rows={performanceRows} />
        <DetailGroup title="Deduções" rows={deductionRows} />
        <DetailGroup title="Resultado final" rows={finalRows} />
      </div>
    </section>
  );
}

function DetailGroup({ title, rows }: { title: string; rows: DetailRow[] }) {
  return (
    <div className={styles.group}>
      <h3 className={styles.groupTitle}>{title}</h3>
      <dl className={styles.list}>
        {rows.map((row) => (
          <div key={row.label} className={styles.row}>
            <dt>{row.label}</dt>
            <dd
              className={
                row.tone === 'positive'
                  ? styles.positive
                  : row.tone === 'negative'
                    ? styles.negative
                    : row.tone === 'muted'
                      ? styles.mutedValue
                      : undefined
              }
            >
              {row.value}
            </dd>
          </div>
        ))}
      </dl>
    </div>
  );
}
