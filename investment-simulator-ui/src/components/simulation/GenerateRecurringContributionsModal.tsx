import { useEffect, useId, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import type { ContributionInput } from '@/types/contribution';
import {
  generateMonthlyWeekdayContributions,
  RECURRING_WEEKDAY_LABELS,
  type RecurringWeekday,
} from '@/utils/generateMonthlyWeekdayContributions';
import styles from './GenerateRecurringContributionsModal.module.css';

const POSITIVE_DECIMAL = /^(?:[1-9]\d*)(?:\.\d+)?$|^0\.\d*[1-9]\d*$/;

const WEEKDAY_OPTIONS = Object.entries(RECURRING_WEEKDAY_LABELS) as Array<
  [RecurringWeekday, string]
>;

export type GenerateRecurringContributionsModalProps = {
  open: boolean;
  startDate: string;
  endDate: string;
  onClose: () => void;
  onGenerate: (contributions: ContributionInput[]) => void;
};

export function GenerateRecurringContributionsModal({
  open,
  startDate,
  endDate,
  onClose,
  onGenerate,
}: GenerateRecurringContributionsModalProps) {
  const titleId = useId();
  const amountId = useId();
  const weekdayId = useId();
  const dialogRef = useRef<HTMLDialogElement>(null);

  const [weekday, setWeekday] = useState<RecurringWeekday>('monday');
  const [amount, setAmount] = useState('');
  const [amountError, setAmountError] = useState<string | null>(null);

  useEffect(() => {
    const dialog = dialogRef.current;
    if (!dialog) {
      return;
    }

    if (open) {
      if (!dialog.open) {
        dialog.showModal();
      }
      setWeekday('monday');
      setAmount('');
      setAmountError(null);
      queueMicrotask(() => {
        dialog.querySelector<HTMLElement>('select, input')?.focus();
      });
    } else if (dialog.open) {
      dialog.close();
    }
  }, [open]);

  const previewCount = generateMonthlyWeekdayContributions({
    startDate,
    endDate,
    weekday,
    amount: '1',
  }).length;

  function handleGenerate() {
    const trimmed = amount.trim();

    if (trimmed === '') {
      setAmountError('Informe o valor do aporte.');
      return;
    }

    if (!POSITIVE_DECIMAL.test(trimmed) || /^0+(?:\.0+)?$/.test(trimmed)) {
      setAmountError('O valor do aporte deve ser maior que zero.');
      return;
    }

    const contributions = generateMonthlyWeekdayContributions({
      startDate,
      endDate,
      weekday,
      amount: trimmed,
    });

    if (contributions.length === 0) {
      setAmountError(
        'Nenhuma data encontrada no período da simulação para o dia escolhido.',
      );
      return;
    }

    onGenerate(contributions);
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
      <div className={styles.form}>
        <h2 id={titleId} className={styles.title}>
          Gerar aportes recorrentes
        </h2>
        <p className={styles.hint}>
          Cria um aporte com o mesmo valor na primeira ocorrência do dia útil
          escolhido em cada mês, entre a data inicial e a data de resgate. As
          linhas atuais da tabela serão substituídas.
        </p>

        <div className={styles.field}>
          <label htmlFor={weekdayId}>Dia útil</label>
          <select
            id={weekdayId}
            value={weekday}
            onChange={(event) =>
              setWeekday(event.target.value as RecurringWeekday)
            }
          >
            {WEEKDAY_OPTIONS.map(([value, label]) => (
              <option key={value} value={value}>
                {label}
              </option>
            ))}
          </select>
        </div>

        <div className={styles.field}>
          <label htmlFor={amountId}>Valor de cada aporte (R$)</label>
          <input
            id={amountId}
            type="text"
            inputMode="decimal"
            autoComplete="off"
            placeholder="0.00"
            value={amount}
            onChange={(event) => {
              setAmount(event.target.value);
              setAmountError(null);
            }}
            onKeyDown={(event) => {
              if (event.key === 'Enter') {
                event.preventDefault();
                handleGenerate();
              }
            }}
            aria-invalid={Boolean(amountError)}
            aria-describedby={
              amountError ? `${amountId}-error` : `${amountId}-preview`
            }
          />
          <p id={`${amountId}-preview`} className={styles.preview}>
            Serão gerados {previewCount} aporte(s) no período.
          </p>
          {amountError ? (
            <p id={`${amountId}-error`} className={styles.error} role="alert">
              {amountError}
            </p>
          ) : null}
        </div>

        <div className={styles.actions}>
          <button type="button" className={styles.cancel} onClick={onClose}>
            Cancelar
          </button>
          <button
            type="button"
            className={styles.confirm}
            onClick={handleGenerate}
          >
            Gerar aportes
          </button>
        </div>
      </div>
    </dialog>,
    document.body,
  );
}
