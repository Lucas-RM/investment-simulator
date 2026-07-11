import { useEffect, useId, useState, type FormEvent } from 'react';
import type {
  RateEntryMode,
  RateScheduleInput,
  TesouroRatesErrors,
  TesouroRatesInput,
} from '@/types/rates';
import {
  createEmptyRateSchedule,
  switchRateScheduleMode,
  syncRateScheduleYears,
} from '@/utils/rateSchedule';
import {
  hasTesouroRatesErrors,
  validateTesouroRates,
} from '@/utils/validateRates';
import { RateScheduleFields } from './RateScheduleFields';
import styles from './RatesForm.module.css';

export type TesouroRatesFormProps = {
  /** Simulation start date (YYYY-MM-DD) — used to generate years. */
  startDate: string;
  /** Redemption date (YYYY-MM-DD) — used to generate years. */
  endDate: string;
  /** Optional initial values. */
  defaultValues?: Partial<TesouroRatesInput>;
  /** Called whenever rate values change (for draft persistence). */
  onValuesChange?: (values: TesouroRatesInput) => void;
  /**
   * Called when the form passes client-side validation.
   * Submit / API wiring is left to later commits.
   */
  onValidSubmit?: (values: TesouroRatesInput) => void;
};

type TesouroScheduleKey = 'selic' | 'b3Custody' | 'ipca';

/** Resolves ágio from current or legacy draft shapes. */
function resolveAnnualAgioRate(defaults?: Partial<TesouroRatesInput>): string {
  if (typeof defaults?.annualAgioRate === 'string') {
    return defaults.annualAgioRate;
  }

  const legacy = defaults as { agio?: RateScheduleInput } | undefined;
  if (legacy?.agio && typeof legacy.agio.singleRate === 'string') {
    return legacy.agio.singleRate;
  }

  return '0';
}

function buildInitialValues(
  startDate: string,
  endDate: string,
  defaults?: Partial<TesouroRatesInput>,
): TesouroRatesInput {
  return {
    selic: syncRateScheduleYears(
      defaults?.selic ?? createEmptyRateSchedule(),
      startDate,
      endDate,
    ),
    annualAgioRate: resolveAnnualAgioRate(defaults),
    b3Custody: syncRateScheduleYears(
      defaults?.b3Custody ?? createEmptyRateSchedule(),
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

export function TesouroRatesForm({
  startDate,
  endDate,
  defaultValues,
  onValuesChange,
  onValidSubmit,
}: TesouroRatesFormProps) {
  const formId = useId();
  const [values, setValues] = useState<TesouroRatesInput>(() =>
    buildInitialValues(startDate, endDate, defaultValues),
  );
  const [errors, setErrors] = useState<TesouroRatesErrors>({});
  const [submitted, setSubmitted] = useState(false);

  function commitValues(next: TesouroRatesInput) {
    setValues(next);
    onValuesChange?.(next);
  }

  useEffect(() => {
    setValues((current) => ({
      ...current,
      selic: syncRateScheduleYears(current.selic, startDate, endDate),
      b3Custody: syncRateScheduleYears(current.b3Custody, startDate, endDate),
      ipca: syncRateScheduleYears(current.ipca, startDate, endDate),
    }));
  }, [startDate, endDate]);

  function updateSchedule(
    key: TesouroScheduleKey,
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
    setSubmitted(false);
  }

  function updateAnnualAgioRate(value: string) {
    commitValues({
      ...values,
      annualAgioRate: value,
    });
    setErrors((current) => {
      if (!current.annualAgioRate) {
        return current;
      }
      const next = { ...current };
      delete next.annualAgioRate;
      return next;
    });
    setSubmitted(false);
  }

  function handleModeChange(key: TesouroScheduleKey, mode: RateEntryMode) {
    updateSchedule(key, (schedule) =>
      switchRateScheduleMode(schedule, mode, startDate, endDate),
    );
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const nextErrors = validateTesouroRates(values, { startDate, endDate });
    setErrors(nextErrors);

    if (hasTesouroRatesErrors(nextErrors)) {
      setSubmitted(false);
      return;
    }

    setSubmitted(true);
    onValidSubmit?.(values);
  }

  const agioId = `${formId}-agio`;

  return (
    <form className={styles.form} onSubmit={handleSubmit} noValidate>
      <fieldset className={styles.fieldset}>
        <legend className={styles.legend}>Taxas — Tesouro Selic</legend>
        <p className={styles.hint}>
          Informe Selic Over, ágio/deságio, custódia B3 e IPCA. Selic, B3 e IPCA
          podem ser taxa única ou ano a ano; o ágio/deságio é um valor decimal
          único.
        </p>

        <RateScheduleFields
          legend="Selic Over anual"
          hint="Taxa Selic Over em percentual ao ano (ex.: 14.15 = 14,15% a.a.)."
          name="selic"
          schedule={values.selic}
          errors={errors.selic}
          onModeChange={(mode) => handleModeChange('selic', mode)}
          onSingleRateChange={(value) =>
            updateSchedule('selic', (schedule) => ({
              ...schedule,
              singleRate: value,
            }))
          }
          onPerYearRatesChange={(rates) =>
            updateSchedule('selic', (schedule) => ({
              ...schedule,
              rates,
            }))
          }
        />

        <div className={styles.field}>
          <label htmlFor={agioId}>Ágio / deságio anual (%)</label>
          <input
            id={agioId}
            name="annualAgioRate"
            type="text"
            inputMode="decimal"
            autoComplete="off"
            placeholder="0.00"
            value={values.annualAgioRate}
            onChange={(event) => updateAnnualAgioRate(event.target.value)}
            aria-invalid={Boolean(errors.annualAgioRate)}
            aria-describedby={
              errors.annualAgioRate
                ? `${agioId}-error ${agioId}-hint`
                : `${agioId}-hint`
            }
          />
          <p id={`${agioId}-hint`} className={styles.fieldHint}>
            Deságio (positivo, ex.: +0,10%): título com desconto — rende Selic
            mais esse percentual. Ágio (negativo, ex.: −0,05%): título acima do
            par — rende Selic menos esse percentual. Ao par: 0,00%.
          </p>
          {errors.annualAgioRate ? (
            <p id={`${agioId}-error`} className={styles.error} role="alert">
              {errors.annualAgioRate}
            </p>
          ) : null}
        </div>

        <RateScheduleFields
          legend="Taxa de custódia B3"
          hint="Custódia B3 em percentual ao ano (ex.: 0.2 = 0,2% a.a.). Use 0 se não houver."
          name="b3Custody"
          schedule={values.b3Custody}
          errors={errors.b3Custody}
          onModeChange={(mode) => handleModeChange('b3Custody', mode)}
          onSingleRateChange={(value) =>
            updateSchedule('b3Custody', (schedule) => ({
              ...schedule,
              singleRate: value,
            }))
          }
          onPerYearRatesChange={(rates) =>
            updateSchedule('b3Custody', (schedule) => ({
              ...schedule,
              rates,
            }))
          }
        />

        <RateScheduleFields
          legend="IPCA anual"
          hint="Inflação (IPCA) em percentual ao ano (ex.: 4.5 = 4,5% a.a.)."
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
        <button type="submit" className={styles.submit}>
          Validar taxas
        </button>
        {submitted ? (
          <p className={styles.success} role="status">
            Taxas válidas. A simulação será conectada em um próximo passo.
          </p>
        ) : null}
      </div>
    </form>
  );
}
