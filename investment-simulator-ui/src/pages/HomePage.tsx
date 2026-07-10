import { Link } from 'react-router-dom'
import { paths } from '@/routes/paths'
import styles from './HomePage.module.css'

export function HomePage() {
  return (
    <section className={styles.section}>
      <h1>Simulador de Investimentos</h1>
      <p>
        Compare rentabilidade, impostos e inflação em CDB pós-fixado e Tesouro
        Selic.
      </p>
      <div className={styles.actions}>
        <Link className={styles.primary} to={paths.cdb}>
          Simular CDB
        </Link>
        <Link className={styles.secondary} to={paths.tesouro}>
          Simular Tesouro Selic
        </Link>
      </div>
    </section>
  )
}
