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

function buildInitialValues(
  startDate: string,
  endDate: string,
  defaults?: Partial<CdbRatesInput>,
): CdbRatesInput {
  const cdi = syncRateScheduleYears(
    defaults?.cdi ?? createEmptyRateSchedule(),
    startDate,
    endDate,
  );

  return {
    profitabilityPercentage: defaults?.profitabilityPercentage ?? '',
    cdi,
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

  function updateCdi(
    updater: (schedule: RateScheduleInput) => RateScheduleInput,
  ) {
    commitValues({
      ...values,
      cdi: updater(values.cdi),
    });
    setErrors((current) => {
      if (!current.cdi) {
        return current;
      }
      const next = { ...current };
      delete next.cdi;
      return next;
    });
  }

  function handleCdiModeChange(mode: RateEntryMode) {
    updateCdi((schedule) =>
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
          Informe a rentabilidade (% do CDI) e a taxa anual do CDI. Use taxa
          única para todo o período ou uma taxa por ano.
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
          onModeChange={handleCdiModeChange}
          onSingleRateChange={(value) =>
            updateCdi((schedule) => ({ ...schedule, singleRate: value }))
          }
          onPerYearRatesChange={(rates) =>
            updateCdi((schedule) => ({
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
