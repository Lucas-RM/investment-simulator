import { useState } from 'react'
import { ContributionsForm } from '@/components/simulation/ContributionsForm'
import { GeneralInputsForm } from '@/components/simulation/GeneralInputsForm'
import type { GeneralInputs } from '@/types/generalInputs'
import { InvestmentType } from '@/types/investment'
import styles from './SimulatorPage.module.css'

export function CdbSimulatorPage() {
  const [generalInputs, setGeneralInputs] = useState<GeneralInputs | null>(
    null,
  )

  return (
    <section className={styles.section}>
      <h1>Simulação CDB</h1>
      <p>
        Preencha as entradas gerais e, em seguida, os aportes adicionais
        da simulação de CDB pós-fixado.
      </p>
      <GeneralInputsForm
        defaultInvestmentType={InvestmentType.Cdb}
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
