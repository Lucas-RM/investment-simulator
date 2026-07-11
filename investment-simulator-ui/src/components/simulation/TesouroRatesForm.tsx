import { useEffect, useState, type FormEvent } from 'react';
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
  /**
   * Called when the form passes client-side validation.
   * Submit / API wiring is left to later commits.
   */
  onValidSubmit?: (values: TesouroRatesInput) => void;
};

type TesouroScheduleKey = keyof TesouroRatesInput;

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
    agio: syncRateScheduleYears(
      defaults?.agio ?? createEmptyRateSchedule(),
      startDate,
      endDate,
    ),
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
  onValidSubmit,
}: TesouroRatesFormProps) {
  const [values, setValues] = useState<TesouroRatesInput>(() =>
    buildInitialValues(startDate, endDate, defaultValues),
  );
  const [errors, setErrors] = useState<TesouroRatesErrors>({});
  const [submitted, setSubmitted] = useState(false);

  useEffect(() => {
    setValues((current) => ({
      selic: syncRateScheduleYears(current.selic, startDate, endDate),
      agio: syncRateScheduleYears(current.agio, startDate, endDate),
      b3Custody: syncRateScheduleYears(current.b3Custody, startDate, endDate),
      ipca: syncRateScheduleYears(current.ipca, startDate, endDate),
    }));
  }, [startDate, endDate]);

  function updateSchedule(
    key: TesouroScheduleKey,
    updater: (schedule: RateScheduleInput) => RateScheduleInput,
  ) {
    setValues((current) => ({
      ...current,
      [key]: updater(current[key]),
    }));
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

  return (
    <form className={styles.form} onSubmit={handleSubmit} noValidate>
      <fieldset className={styles.fieldset}>
        <legend className={styles.legend}>Taxas — Tesouro Selic</legend>
        <p className={styles.hint}>
          Informe Selic Over, ágio/deságio, custódia B3 e IPCA. Cada taxa pode
          ser única para todo o período ou preenchida ano a ano.
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
          onYearRateChange={(year, value) =>
            updateSchedule('selic', (schedule) => ({
              ...schedule,
              rates: schedule.rates.map((entry) =>
                entry.year === year ? { ...entry, rate: value } : entry,
              ),
            }))
          }
        />

        <RateScheduleFields
          legend="Ágio / deságio anual"
          hint="Positivo = ágio; negativo = deságio (ex.: 0.1 ou -0.1)."
          name="agio"
          schedule={values.agio}
          errors={errors.agio}
          allowNegative
          onModeChange={(mode) => handleModeChange('agio', mode)}
          onSingleRateChange={(value) =>
            updateSchedule('agio', (schedule) => ({
              ...schedule,
              singleRate: value,
            }))
          }
          onYearRateChange={(year, value) =>
            updateSchedule('agio', (schedule) => ({
              ...schedule,
              rates: schedule.rates.map((entry) =>
                entry.year === year ? { ...entry, rate: value } : entry,
              ),
            }))
          }
        />

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
          onYearRateChange={(year, value) =>
            updateSchedule('b3Custody', (schedule) => ({
              ...schedule,
              rates: schedule.rates.map((entry) =>
                entry.year === year ? { ...entry, rate: value } : entry,
              ),
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
          onYearRateChange={(year, value) =>
            updateSchedule('ipca', (schedule) => ({
              ...schedule,
              rates: schedule.rates.map((entry) =>
                entry.year === year ? { ...entry, rate: value } : entry,
              ),
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
