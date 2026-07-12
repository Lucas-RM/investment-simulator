import { useEffect, useId, useRef } from 'react';
import { createPortal } from 'react-dom';
import type { ContributionDetailResponse } from '@/types/simulationApi';
import styles from './ContributionDetailsModal.module.css';

export type ContributionDetailsModalProps = {
  open: boolean;
  details: ContributionDetailResponse[];
  onClose: () => void;
};

function formatMoney(value: number): string {
  return value.toLocaleString('pt-BR', {
    style: 'currency',
    currency: 'BRL',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
}

function formatDate(isoDate: string): string {
  const [year, month, day] = isoDate.split('-').map(Number);
  if (!year || !month || !day) {
    return isoDate;
  }

  return new Date(Date.UTC(year, month - 1, day)).toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    timeZone: 'UTC',
  });
}

/**
 * Modal table with per-contribution breakdown (ERS §20).
 */
export function ContributionDetailsModal({
  open,
  details,
  onClose,
}: ContributionDetailsModalProps) {
  const titleId = useId();
  const dialogRef = useRef<HTMLDialogElement>(null);

  useEffect(() => {
    const dialog = dialogRef.current;
    if (!dialog) {
      return;
    }

    if (open) {
      if (!dialog.open) {
        dialog.showModal();
        queueMicrotask(() => {
          dialog.querySelector<HTMLElement>('button')?.focus();
        });
      }
      return;
    }

    if (dialog.open) {
      dialog.close();
    }
  }, [open]);

  if (!open) {
    return null;
  }

  return createPortal(
    <dialog
      ref={dialogRef}
      className={styles.dialog}
      aria-labelledby={titleId}
      onClose={onClose}
      onCancel={(event) => {
        event.preventDefault();
        onClose();
      }}
    >
      <div className={styles.body}>
        <h2 id={titleId} className={styles.title}>
          Detalhamento por aporte
        </h2>
        <p className={styles.hint}>
          Cada aporte é tratado de forma independente para cálculo de
          rendimento, IR e IOF.
        </p>

        {details.length === 0 ? (
          <p className={styles.empty}>Nenhum aporte detalhado nesta simulação.</p>
        ) : (
          <div className={styles.tableWrap}>
            <table className={styles.table}>
              <thead>
                <tr>
                  <th scope="col">Data</th>
                  <th scope="col">Valor</th>
                  <th scope="col">Saldo bruto</th>
                  <th scope="col">Rendimento bruto</th>
                  <th scope="col">Dias corridos</th>
                  <th scope="col">Dias úteis</th>
                  <th scope="col">IR</th>
                  <th scope="col">IOF</th>
                </tr>
              </thead>
              <tbody>
                {details.map((detail, index) => (
                  <tr key={`${detail.date}-${detail.amount}-${index}`}>
                    <td>{formatDate(detail.date)}</td>
                    <td>{formatMoney(detail.amount)}</td>
                    <td>{formatMoney(detail.grossBalance)}</td>
                    <td>{formatMoney(detail.grossYield)}</td>
                    <td>{detail.calendarDaysInvested}</td>
                    <td>{detail.businessDaysInvested}</td>
                    <td>{formatMoney(detail.incomeTax)}</td>
                    <td>{formatMoney(detail.iof)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        <div className={styles.actions}>
          <button type="button" className={styles.close} onClick={onClose}>
            Fechar
          </button>
        </div>
      </div>
    </dialog>,
    document.body,
  );
}
