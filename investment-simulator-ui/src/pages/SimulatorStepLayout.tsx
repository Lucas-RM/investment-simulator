import type { ReactNode } from 'react';
import { Link } from 'react-router-dom';
import styles from './SimulatorStepLayout.module.css';

export type SimulatorStepLayoutProps = {
  title: string;
  description: string;
  /** Optional link back to the previous wizard step (or home). */
  backTo?: string;
  backLabel?: string;
  /** Wider content area (e.g. simulation result page). */
  wide?: boolean;
  children: ReactNode;
};

export function SimulatorStepLayout({
  title,
  description,
  backTo,
  backLabel = 'Voltar',
  wide = false,
  children,
}: SimulatorStepLayoutProps) {
  return (
    <section className={wide ? `${styles.section} ${styles.wide}` : styles.section}>
      {backTo ? (
        <div className={styles.back}>
          <Link to={backTo} className={styles.backButton}>
            ← {backLabel}
          </Link>
        </div>
      ) : null}
      <h1>{title}</h1>
      <p className={styles.description}>{description}</p>
      {children}
    </section>
  );
}
