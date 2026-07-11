import { useEffect, useId, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import type { AnnualRateInput } from '@/types/rates';
import styles from './PerYearRatesModal.module.css';

const NON_NEGATIVE_DECIMAL = /^(?:0|[1-9]\d*)(?:\.\d+)?$/;
const SIGNED_DECIMAL = /^-?(?:0|[1-9]\d*)(?:\.\d+)?$/;

export type PerYearRatesModalProps = {
  open: boolean;
  /** Modal title, usually the schedule legend (e.g. "CDI anual"). */
  title: string;
  /** Optional hint shown under the title. */
  hint?: string;
  rates: AnnualRateInput[];
  /** When true, rate inputs accept a leading minus. */
  allowNegative?: boolean;
  onClose: () => void;
  onApply: (rates: AnnualRateInput[]) => void;
};

function validateRate(raw: string, allowNegative: boolean): string | undefined {
  const value = raw.trim();

  if (value === '') {
    return 'Informe a taxa anual.';
  }

  if (allowNegative) {
    if (!SIGNED_DECIMAL.test(value)) {
      return 'Informe uma taxa válida.';
    }

    const numeric = Number(value);
    if (Number.isNaN(numeric) || numeric <= -100) {
      return 'A taxa deve ser maior que -100%.';
    }

    return undefined;
  }

  if (!NON_NEGATIVE_DECIMAL.test(value)) {
    return 'A taxa não pode ser negativa.';
  }

  return undefined;
}

export function PerYearRatesModal({
  open,
  title,
  hint,
  rates,
  allowNegative = false,
  onClose,
  onApply,
}: PerYearRatesModalProps) {
  const titleId = useId();
  const fieldId = useId();
  const dialogRef = useRef<HTMLDialogElement>(null);

  const [draftRates, setDraftRates] = useState<AnnualRateInput[]>(rates);
  const [errors, setErrors] = useState<Record<number, string>>({});
  const [formError, setFormError] = useState<string | null>(null);

  useEffect(() => {
    const dialog = dialogRef.current;
    if (!dialog) {
      return;
    }

    if (open) {
      if (!dialog.open) {
        dialog.showModal();
        setDraftRates(rates.map((entry) => ({ ...entry })));
        setErrors({});
        setFormError(null);
        queueMicrotask(() => {
          dialog.querySelector<HTMLElement>('input')?.focus();
        });
      }
    } else if (dialog.open) {
      dialog.close();
    }
  }, [open, rates]);

  function updateRate(year: number, value: string) {
    setDraftRates((current) =>
      current.map((entry) =>
        entry.year === year ? { ...entry, rate: value } : entry,
      ),
    );
    setErrors((current) => {
      if (!current[year]) {
        return current;
      }
      const next = { ...current };
      delete next[year];
      return next;
    });
    setFormError(null);
  }

  function handleApply() {
    if (draftRates.length === 0) {
      setFormError('Não há anos no período para informar taxas.');
      return;
    }

    const nextErrors: Record<number, string> = {};
    for (const entry of draftRates) {
      const error = validateRate(entry.rate, allowNegative);
      if (error) {
        nextErrors[entry.year] = error;
      }
    }

    setErrors(nextErrors);

    if (Object.keys(nextErrors).length > 0) {
      setFormError('Corrija as taxas destacadas antes de aplicar.');
      return;
    }

    onApply(
      draftRates.map((entry) => ({
        year: entry.year,
        rate: entry.rate.trim(),
      })),
    );
    onClose();
  }

  return createPortal(
    <dialog
      ref={dialogRef}
      className={styles.dialog}
      aria-labelledby={titleId}
      onCancel={(event) => {
        event.preventDefault();
        onClose();
      }}
    >
      <div className={styles.body}>
        <h2 id={titleId} className={styles.title}>
          {title} — ano a ano
        </h2>
        <p className={styles.hint}>
          {hint ??
            'Informe a taxa anual (%) para cada ano do período da simulação.'}
        </p>

        {formError ? (
          <p className={styles.formError} role="alert">
            {formError}
          </p>
        ) : null}

        {draftRates.length === 0 ? (
          <p className={styles.hint}>
            Nenhum ano disponível. Verifique as datas da simulação.
          </p>
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
                {draftRates.map((entry) => {
                  const rateId = `${fieldId}-year-${entry.year}`;
                  const yearError = errors[entry.year];

                  return (
                    <tr key={entry.year}>
                      <td className={styles.yearCell}>{entry.year}</td>
                      <td>
                        <label className={styles.srOnly} htmlFor={rateId}>
                          Taxa de {title} em {entry.year}
                        </label>
                        <input
                          id={rateId}
                          type="text"
                          inputMode="decimal"
                          autoComplete="off"
                          placeholder={allowNegative ? '0.00' : '15.00'}
                          value={entry.rate}
                          onChange={(event) =>
                            updateRate(entry.year, event.target.value)
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

        <div className={styles.actions}>
          <button type="button" className={styles.cancel} onClick={onClose}>
            Cancelar
          </button>
          <button
            type="button"
            className={styles.confirm}
            onClick={handleApply}
          >
            Aplicar taxas
          </button>
        </div>
      </div>
    </dialog>,
    document.body,
  );
}
