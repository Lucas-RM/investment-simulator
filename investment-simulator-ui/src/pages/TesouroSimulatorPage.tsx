import { GeneralInputsForm } from '@/components/simulation/GeneralInputsForm'
import { InvestmentType } from '@/types/investment'
import styles from './SimulatorPage.module.css'

export function TesouroSimulatorPage() {
  return (
    <section className={styles.section}>
      <h1>Simulação Tesouro Selic</h1>
      <p>
        Preencha as entradas gerais para iniciar a simulação do Tesouro
        Selic.
      </p>
      <GeneralInputsForm
        defaultInvestmentType={InvestmentType.TesouroSelic}
      />
    </section>
  )
}
