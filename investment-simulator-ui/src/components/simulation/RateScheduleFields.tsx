import { useId } from 'react';
import type {
  RateEntryMode,
  RateScheduleErrors,
  RateScheduleInput,
} from '@/types/rates';
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
  /** When true, rate inputs accept a leading minus (ágio/deságio). */
  allowNegative?: boolean;
  onModeChange: (mode: RateEntryMode) => void;
  onSingleRateChange: (value: string) => void;
  onYearRateChange: (year: number, value: string) => void;
};

export function RateScheduleFields({
  legend,
  hint,
  name,
  schedule,
  errors,
  allowNegative = false,
  onModeChange,
  onSingleRateChange,
  onYearRateChange,
}: RateScheduleFieldsProps) {
  const fieldId = useId();
  const singleId = `${fieldId}-single`;
  const modeGroupId = `${fieldId}-mode`;

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
        <div className={styles.tableWrap}>
          <table className={styles.table}>
            <thead>
              <tr>
                <th scope="col">Ano</th>
                <th scope="col">Taxa anual (%)</th>
              </tr>
            </thead>
            <tbody>
              {schedule.rates.map((entry) => {
                const rateId = `${fieldId}-year-${entry.year}`;
                const yearError = errors?.rates?.[entry.year];

                return (
                  <tr key={entry.year}>
                    <td className={styles.yearCell}>{entry.year}</td>
                    <td>
                      <label className={styles.srOnly} htmlFor={rateId}>
                        Taxa de {legend} em {entry.year}
                      </label>
                      <input
                        id={rateId}
                        name={`${name}.rates.${entry.year}`}
                        type="text"
                        inputMode="decimal"
                        autoComplete="off"
                        placeholder={allowNegative ? '0.00' : '15.00'}
                        value={entry.rate}
                        onChange={(event) =>
                          onYearRateChange(entry.year, event.target.value)
                        }
                        aria-invalid={Boolean(yearError)}
                        aria-describedby={
                          yearError ? `${rateId}-error` : undefined
                        }
                      />
                      {yearError ? (
                        <p
                          id={`${rateId}-error`}
                          className={styles.error}
                          role="alert"
                        >
                          {yearError}
                        </p>
                      ) : null}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}
    </fieldset>
  );
}
