import type { ReactNode } from 'react';
import { Link } from 'react-router-dom';
import styles from './SimulatorStepLayout.module.css';

export type SimulatorStepLayoutProps = {
  title: string;
  description: string;
  /** Optional link back to the previous wizard step. */
  backTo?: string;
  backLabel?: string;
  children: ReactNode;
};

export function SimulatorStepLayout({
  title,
  description,
  backTo,
  backLabel = 'Voltar',
  children,
}: SimulatorStepLayoutProps) {
  return (
    <section className={styles.section}>
      {backTo ? (
        <p className={styles.back}>
          <Link to={backTo}>{backLabel}</Link>
        </p>
      ) : null}
      <h1>{title}</h1>
      <p className={styles.description}>{description}</p>
      {children}
    </section>
  );
}
