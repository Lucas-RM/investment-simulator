import { useId, useState } from 'react';
import type {
  AnnualRateInput,
  RateEntryMode,
  RateScheduleErrors,
  RateScheduleInput,
} from '@/types/rates';
import { PerYearRatesModal } from './PerYearRatesModal';
import styles from './RateScheduleFields.module.css';

export type RateScheduleFieldsProps = {
  /** Accessible name for this schedule (e.g. "CDI anual"). */
  legend: string;
  /** Optional short hint under the legend. */
  hint?: string;
  /** Form field name prefix (e.g. "cdi"). */
  name: string;
  schedule: RateScheduleInput;
  errors?: RateScheduleErrors;
  /** When true, rate inputs accept a leading minus. */
  allowNegative?: boolean;
  onModeChange: (mode: RateEntryMode) => void;
  onSingleRateChange: (value: string) => void;
  /** Replaces the full per-year rate list (from the modal). */
  onPerYearRatesChange: (rates: AnnualRateInput[]) => void;
};

function hasAnyFilledRate(rates: AnnualRateInput[]): boolean {
  return rates.some((entry) => entry.rate.trim() !== '');
}

export function RateScheduleFields({
  legend,
  hint,
  name,
  schedule,
  errors,
  allowNegative = false,
  onModeChange,
  onSingleRateChange,
  onPerYearRatesChange,
}: RateScheduleFieldsProps) {
  const fieldId = useId();
  const singleId = `${fieldId}-single`;
  const modeGroupId = `${fieldId}-mode`;
  const [modalOpen, setModalOpen] = useState(false);

  const filledCount = schedule.rates.filter(
    (entry) => entry.rate.trim() !== '',
  ).length;

  return (
    <fieldset className={styles.schedule}>
      <legend className={styles.title}>{legend}</legend>
      {hint ? <p className={styles.hint}>{hint}</p> : null}

      <div
        className={styles.modeToggle}
        role="radiogroup"
        aria-labelledby={modeGroupId}
      >
        <span id={modeGroupId} className={styles.srOnly}>
          Modo de entrada de {legend}
        </span>
        <label className={styles.modeOption}>
          <input
            type="radio"
            name={`${name}.mode`}
            value="single"
            checked={schedule.mode === 'single'}
            onChange={() => onModeChange('single')}
          />
          Taxa única
        </label>
        <label className={styles.modeOption}>
          <input
            type="radio"
            name={`${name}.mode`}
            value="perYear"
            checked={schedule.mode === 'perYear'}
            onChange={() => onModeChange('perYear')}
          />
          Ano a ano
        </label>
      </div>

      {errors?.mode ? (
        <p className={styles.error} role="alert">
          {errors.mode}
        </p>
      ) : null}

      {schedule.mode === 'single' ? (
        <div className={styles.field}>
          <label htmlFor={singleId}>Taxa anual (%)</label>
          <input
            id={singleId}
            name={`${name}.singleRate`}
            type="text"
            inputMode="decimal"
            autoComplete="off"
            placeholder={allowNegative ? '0.00' : '15.00'}
            value={schedule.singleRate}
            onChange={(event) => onSingleRateChange(event.target.value)}
            aria-invalid={Boolean(errors?.singleRate)}
            aria-describedby={
              errors?.singleRate ? `${singleId}-error` : undefined
            }
          />
          {errors?.singleRate ? (
            <p id={`${singleId}-error`} className={styles.error} role="alert">
              {errors.singleRate}
            </p>
          ) : null}
        </div>
      ) : (
        <div className={styles.perYearPanel}>
          <p className={styles.summary}>
            {filledCount === 0
              ? 'Nenhuma taxa ano a ano informada ainda.'
              : `${filledCount} de ${schedule.rates.length} ano(s) com taxa informada.`}
          </p>

          {schedule.rates.length > 0 && filledCount > 0 ? (
            <ul className={styles.rateList}>
              {schedule.rates.map((entry) => (
                <li key={entry.year}>
                  <span>{entry.year}</span>
                  <span>
                    {entry.rate.trim() === '' ? '—' : `${entry.rate}%`}
                  </span>
                </li>
              ))}
            </ul>
          ) : null}

          <button
            type="button"
            className={styles.openModal}
            onClick={() => setModalOpen(true)}
          >
            {hasAnyFilledRate(schedule.rates)
              ? 'Editar taxas ano a ano'
              : 'Informar taxas ano a ano'}
          </button>

          {errors?.rates
            ? Object.entries(errors.rates).map(([year, message]) => (
                <p key={year} className={styles.error} role="alert">
                  {year}: {message}
                </p>
              ))
            : null}
        </div>
      )}

      <PerYearRatesModal
        open={modalOpen}
        title={legend}
        hint={hint}
        rates={schedule.rates}
        allowNegative={allowNegative}
        onClose={() => setModalOpen(false)}
        onApply={onPerYearRatesChange}
      />
    </fieldset>
  );
}
