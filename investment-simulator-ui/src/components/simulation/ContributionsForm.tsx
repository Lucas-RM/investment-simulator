import { useId, useState, type FormEvent } from 'react';
import type {
  ContributionInput,
  ContributionRow,
  ContributionsErrors,
} from '@/types/contribution';
import {
  hasContributionsErrors,
  validateContributions,
} from '@/utils/validateContributions';
import { GenerateRecurringContributionsModal } from './GenerateRecurringContributionsModal';
import styles from './ContributionsForm.module.css';

export type ContributionsFormProps = {
  /** First contribution / simulation start date (YYYY-MM-DD). */
  startDate: string;
  /** Redemption (end) date (YYYY-MM-DD). */
  endDate: string;
  /** Optional initial rows (without client ids). */
  defaultContributions?: ContributionInput[];
  /**
   * Called when the form passes client-side validation.
   * Submit / API wiring is left to later commits.
   */
  onValidSubmit?: (contributions: ContributionInput[]) => void;
};

let rowIdCounter = 0;

function createRowId(): string {
  rowIdCounter += 1;
  return `contribution-${rowIdCounter}`;
}

function toRows(contributions: ContributionInput[] = []): ContributionRow[] {
  return contributions.map((item) => ({
    id: createRowId(),
    date: item.date,
    amount: item.amount,
  }));
}

function emptyRow(): ContributionRow {
  return { id: createRowId(), date: '', amount: '' };
}

export function ContributionsForm({
  startDate,
  endDate,
  defaultContributions,
  onValidSubmit,
}: ContributionsFormProps) {
  const formId = useId();
  const [rows, setRows] = useState<ContributionRow[]>(() =>
    toRows(defaultContributions),
  );
  const [errors, setErrors] = useState<ContributionsErrors>({});
  const [submitted, setSubmitted] = useState(false);
  const [generatorOpen, setGeneratorOpen] = useState(false);

  function clearRowFieldError(rowId: string, field: 'date' | 'amount') {
    setErrors((current) => {
      const rowErrors = current[rowId];
      if (!rowErrors?.[field]) {
        return current;
      }

      const nextRow = { ...rowErrors };
      delete nextRow[field];

      const next = { ...current };
      if (Object.keys(nextRow).length === 0) {
        delete next[rowId];
      } else {
        next[rowId] = nextRow;
      }
      return next;
    });
  }

  function updateRow(rowId: string, field: 'date' | 'amount', value: string) {
    setRows((current) =>
      current.map((row) =>
        row.id === rowId ? { ...row, [field]: value } : row,
      ),
    );
    clearRowFieldError(rowId, field);
    setSubmitted(false);
  }

  function addRow() {
    setRows((current) => [...current, emptyRow()]);
    setSubmitted(false);
  }

  function applyGeneratedContributions(contributions: ContributionInput[]) {
    setRows(toRows(contributions));
    setErrors({});
    setSubmitted(false);
  }

  function removeRow(rowId: string) {
    setRows((current) => current.filter((row) => row.id !== rowId));
    setErrors((current) => {
      if (!current[rowId]) {
        return current;
      }
      const next = { ...current };
      delete next[rowId];
      return next;
    });
    setSubmitted(false);
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const nextErrors = validateContributions(rows, { startDate, endDate });
    setErrors(nextErrors);

    if (hasContributionsErrors(nextErrors)) {
      setSubmitted(false);
      return;
    }

    const contributions: ContributionInput[] = rows.map(({ date, amount }) => ({
      date,
      amount: amount.trim(),
    }));

    setSubmitted(true);
    onValidSubmit?.(contributions);
  }

  return (
    <div className={styles.form}>
      <form onSubmit={handleSubmit} noValidate>
        <fieldset className={styles.fieldset}>
          <legend className={styles.legend}>Aportes adicionais</legend>
          <p className={styles.hint}>
            Adicione quantos aportes quiser (ou nenhum). Cada linha precisa de
            data e valor maiores que zero, dentro do período da simulação.
          </p>

          {rows.length === 0 ? (
            <p className={styles.empty}>Nenhum aporte adicional cadastrado.</p>
          ) : (
            <div className={styles.tableWrap}>
              <table className={styles.table}>
                <thead>
                  <tr>
                    <th scope="col">Data</th>
                    <th scope="col">Valor (R$)</th>
                    <th scope="col">
                      <span className={styles.srOnly}>Ações</span>
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {rows.map((row, index) => {
                    const dateId = `${formId}-date-${row.id}`
                    const amountId = `${formId}-amount-${row.id}`
                    const rowErrors = errors[row.id]
                    const dateErrorId = `${dateId}-error`
                    const amountErrorId = `${amountId}-error`

                    return (
                      <tr key={row.id}>
                        <td>
                          <label className={styles.srOnly} htmlFor={dateId}>
                            Data do aporte {index + 1}
                          </label>
                          <input
                            id={dateId}
                            name={`contributions[${index}].date`}
                            type="date"
                            value={row.date}
                            min={startDate || undefined}
                            max={endDate || undefined}
                            onChange={(event) =>
                              updateRow(row.id, 'date', event.target.value)
                            }
                            aria-invalid={Boolean(rowErrors?.date)}
                            aria-describedby={
                              rowErrors?.date ? dateErrorId : undefined
                            }
                          />
                          {rowErrors?.date ? (
                            <p
                              id={dateErrorId}
                              className={styles.error}
                              role="alert"
                            >
                              {rowErrors.date}
                            </p>
                          ) : null}
                        </td>
                        <td>
                          <label
                            className={styles.srOnly}
                            htmlFor={amountId}
                          >
                            Valor do aporte {index + 1}
                          </label>
                          <input
                            id={amountId}
                            name={`contributions[${index}].amount`}
                            type="text"
                            inputMode="decimal"
                            autoComplete="off"
                            placeholder="0.00"
                            value={row.amount}
                            onChange={(event) =>
                              updateRow(row.id, 'amount', event.target.value)
                            }
                            aria-invalid={Boolean(rowErrors?.amount)}
                            aria-describedby={
                              rowErrors?.amount ? amountErrorId : undefined
                            }
                          />
                          {rowErrors?.amount ? (
                            <p
                              id={amountErrorId}
                              className={styles.error}
                              role="alert"
                            >
                              {rowErrors.amount}
                            </p>
                          ) : null}
                        </td>
                        <td className={styles.actionsCell}>
                          <button
                            type="button"
                            className={styles.remove}
                            onClick={() => removeRow(row.id)}
                            aria-label={`Remover aporte ${index + 1}`}
                          >
                            Remover
                          </button>
                        </td>
                      </tr>
                    )
                  })}
                </tbody>
              </table>
            </div>
          )}

          <div className={styles.toolbar}>
            <button type="button" className={styles.add} onClick={addRow}>
              Adicionar aporte
            </button>
            <button
              type="button"
              className={styles.generate}
              onClick={() => setGeneratorOpen(true)}
            >
              Gerar aportes recorrentes
            </button>
          </div>
        </fieldset>

        <div className={styles.actions}>
          <button type="submit" className={styles.submit}>
            Validar aportes
          </button>
          {submitted ? (
            <p className={styles.success} role="status">
              Aportes válidos. Próximos passos (taxas) serão adicionados em
              seguida.
            </p>
          ) : null}
        </div>
      </form>

      <GenerateRecurringContributionsModal
        open={generatorOpen}
        startDate={startDate}
        endDate={endDate}
        onClose={() => setGeneratorOpen(false)}
        onGenerate={applyGeneratedContributions}
      />
    </div>
  )
}
