import { GeneralInputsForm } from '@/components/simulation/GeneralInputsForm'
import { InvestmentType } from '@/types/investment'
import styles from './SimulatorPage.module.css'

export function CdbSimulatorPage() {
  return (
    <section className={styles.section}>
      <h1>Simulação CDB</h1>
      <p>
        Preencha as entradas gerais para iniciar a simulação de CDB
        pós-fixado.
      </p>
      <GeneralInputsForm defaultInvestmentType={InvestmentType.Cdb} />
    </section>
  )
}
