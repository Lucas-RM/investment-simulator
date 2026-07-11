import { useEffect, useId, useState, type FormEvent } from 'react';
import type {
  CdbRatesErrors,
  CdbRatesInput,
  RateEntryMode,
  RateScheduleInput,
} from '@/types/rates';
import {
  createEmptyRateSchedule,
  switchRateScheduleMode,
  syncRateScheduleYears,
} from '@/utils/rateSchedule';
import { hasCdbRatesErrors, validateCdbRates } from '@/utils/validateRates';
import { RateScheduleFields } from './RateScheduleFields';
import styles from './RatesForm.module.css';

export type CdbRatesFormProps = {
  /** Simulation start date (YYYY-MM-DD) — used to generate years. */
  startDate: string;
  /** Redemption date (YYYY-MM-DD) — used to generate years. */
  endDate: string;
  /** Optional initial values. */
  defaultValues?: Partial<CdbRatesInput>;
  /** Called whenever rate values change (for draft persistence). */
  onValuesChange?: (values: CdbRatesInput) => void;
  /**
   * Called when the form passes client-side validation.
   * May return a Promise when wired to the simulation API.
   */
  onValidSubmit?: (values: CdbRatesInput) => void | Promise<void>;
  /** Disables the submit button while the API request is in flight. */
  isSubmitting?: boolean;
  /** API / server error message shown below the submit button. */
  submitError?: string | null;
};

type CdbScheduleKey = 'cdi' | 'ipca';

function buildInitialValues(
  startDate: string,
  endDate: string,
  defaults?: Partial<CdbRatesInput>,
): CdbRatesInput {
  return {
    profitabilityPercentage: defaults?.profitabilityPercentage ?? '',
    cdi: syncRateScheduleYears(
      defaults?.cdi ?? createEmptyRateSchedule(),
      startDate,
      endDate,
    ),
    ipca: syncRateScheduleYears(
      defaults?.ipca ?? createEmptyRateSchedule(),
      startDate,
      endDate,
    ),
  };
}

export function CdbRatesForm({
  startDate,
  endDate,
  defaultValues,
  onValuesChange,
  onValidSubmit,
  isSubmitting = false,
  submitError = null,
}: CdbRatesFormProps) {
  const formId = useId();
  const [values, setValues] = useState<CdbRatesInput>(() =>
    buildInitialValues(startDate, endDate, defaultValues),
  );
  const [errors, setErrors] = useState<CdbRatesErrors>({});

  function commitValues(next: CdbRatesInput) {
    setValues(next);
    onValuesChange?.(next);
  }

  useEffect(() => {
    setValues((current) => ({
      ...current,
      cdi: syncRateScheduleYears(current.cdi, startDate, endDate),
      ipca: syncRateScheduleYears(current.ipca, startDate, endDate),
    }));
  }, [startDate, endDate]);

  function updateProfitability(value: string) {
    commitValues({
      ...values,
      profitabilityPercentage: value,
    });
    setErrors((current) => {
      if (!current.profitabilityPercentage) {
        return current;
      }
      const next = { ...current };
      delete next.profitabilityPercentage;
      return next;
    });
  }

  function updateSchedule(
    key: CdbScheduleKey,
    updater: (schedule: RateScheduleInput) => RateScheduleInput,
  ) {
    commitValues({
      ...values,
      [key]: updater(values[key]),
    });
    setErrors((current) => {
      if (!current[key]) {
        return current;
      }
      const next = { ...current };
      delete next[key];
      return next;
    });
  }

  function handleModeChange(key: CdbScheduleKey, mode: RateEntryMode) {
    updateSchedule(key, (schedule) =>
      switchRateScheduleMode(schedule, mode, startDate, endDate),
    );
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (isSubmitting) {
      return;
    }

    const nextErrors = validateCdbRates(values, { startDate, endDate });
    setErrors(nextErrors);

    if (hasCdbRatesErrors(nextErrors)) {
      return;
    }

    await onValidSubmit?.(values);
  }

  const profitabilityId = `${formId}-profitability`;

  return (
    <form className={styles.form} onSubmit={handleSubmit} noValidate>
      <fieldset className={styles.fieldset} disabled={isSubmitting}>
        <legend className={styles.legend}>Taxas — CDB</legend>
        <p className={styles.hint}>
          Informe a rentabilidade (% do CDI), a taxa anual do CDI e o IPCA. Use
          taxa única para todo o período ou uma taxa por ano.
        </p>

        <div className={styles.field}>
          <label htmlFor={profitabilityId}>Rentabilidade (% do CDI)</label>
          <input
            id={profitabilityId}
            name="profitabilityPercentage"
            type="text"
            inputMode="decimal"
            autoComplete="off"
            placeholder="120"
            value={values.profitabilityPercentage}
            onChange={(event) => updateProfitability(event.target.value)}
            aria-invalid={Boolean(errors.profitabilityPercentage)}
            aria-describedby={
              errors.profitabilityPercentage
                ? `${profitabilityId}-error ${profitabilityId}-hint`
                : `${profitabilityId}-hint`
            }
          />
          <p id={`${profitabilityId}-hint`} className={styles.fieldHint}>
            Ex.: 100 = 100% do CDI; 120 = 120% do CDI.
          </p>
          {errors.profitabilityPercentage ? (
            <p
              id={`${profitabilityId}-error`}
              className={styles.error}
              role="alert"
            >
              {errors.profitabilityPercentage}
            </p>
          ) : null}
        </div>

        <RateScheduleFields
          legend="CDI anual"
          hint="Taxa do CDI em percentual ao ano (ex.: 15 = 15% a.a.)."
          name="cdi"
          schedule={values.cdi}
          errors={errors.cdi}
          onModeChange={(mode) => handleModeChange('cdi', mode)}
          onSingleRateChange={(value) =>
            updateSchedule('cdi', (schedule) => ({
              ...schedule,
              singleRate: value,
            }))
          }
          onPerYearRatesChange={(rates) =>
            updateSchedule('cdi', (schedule) => ({
              ...schedule,
              rates,
            }))
          }
        />

        <RateScheduleFields
          legend="IPCA anual"
          hint="Inflação (IPCA) em percentual ao ano — IPCA oficial acumulado dos últimos 12 meses ou projeção/expectativa para os próximos anos (ex.: 4.5 = 4,5% a.a.)."
          name="ipca"
          schedule={values.ipca}
          errors={errors.ipca}
          onModeChange={(mode) => handleModeChange('ipca', mode)}
          onSingleRateChange={(value) =>
            updateSchedule('ipca', (schedule) => ({
              ...schedule,
              singleRate: value,
            }))
          }
          onPerYearRatesChange={(rates) =>
            updateSchedule('ipca', (schedule) => ({
              ...schedule,
              rates,
            }))
          }
        />
      </fieldset>

      <div className={styles.actions}>
        <button
          type="submit"
          className={styles.submit}
          disabled={isSubmitting}
          aria-busy={isSubmitting}
        >
          {isSubmitting ? 'Simulando…' : 'Simular'}
        </button>
        {isSubmitting ? (
          <p className={styles.status} role="status" aria-live="polite">
            Calculando a simulação…
          </p>
        ) : null}
        {submitError ? (
          <p className={styles.error} role="alert">
            {submitError}
          </p>
        ) : null}
      </div>
    </form>
  );
}
