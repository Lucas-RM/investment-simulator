import { useState } from 'react'
import { ContributionsForm } from '@/components/simulation/ContributionsForm'
import { GeneralInputsForm } from '@/components/simulation/GeneralInputsForm'
import type { GeneralInputs } from '@/types/generalInputs'
import { InvestmentType } from '@/types/investment'
import styles from './SimulatorPage.module.css'

export function TesouroSimulatorPage() {
  const [generalInputs, setGeneralInputs] = useState<GeneralInputs | null>(
    null,
  )

  return (
    <section className={styles.section}>
      <h1>Simulação Tesouro Selic</h1>
      <p>
        Preencha as entradas gerais e, em seguida, os aportes adicionais
        da simulação do Tesouro Selic.
      </p>
      <GeneralInputsForm
        defaultInvestmentType={InvestmentType.TesouroSelic}
        onValidSubmit={setGeneralInputs}
      />
      {generalInputs ? (
        <div className={styles.followUp}>
          <ContributionsForm
            startDate={generalInputs.startDate}
            endDate={generalInputs.endDate}
          />
        </div>
      ) : null}
    </section>
  )
}
