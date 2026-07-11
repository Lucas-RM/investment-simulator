import { useState } from 'react';
import { ContributionsForm } from '@/components/simulation/ContributionsForm';
import { GeneralInputsForm } from '@/components/simulation/GeneralInputsForm';
import { TesouroRatesForm } from '@/components/simulation/TesouroRatesForm';
import type { ContributionInput } from '@/types/contribution';
import type { GeneralInputs } from '@/types/generalInputs';
import { InvestmentType } from '@/types/investment';
import styles from './SimulatorPage.module.css';

export function TesouroSimulatorPage() {
  const [generalInputs, setGeneralInputs] = useState<GeneralInputs | null>(
    null,
  );
  const [contributions, setContributions] = useState<
    ContributionInput[] | null
  >(null);

  function handleGeneralInputsSubmit(values: GeneralInputs) {
    setGeneralInputs(values);
    setContributions(null);
  }

  return (
    <section className={styles.section}>
      <h1>Simulação Tesouro Selic</h1>
      <p>
        Preencha as entradas gerais, os aportes adicionais e as taxas da
        simulação do Tesouro Selic.
      </p>
      <GeneralInputsForm
        defaultInvestmentType={InvestmentType.TesouroSelic}
        onValidSubmit={handleGeneralInputsSubmit}
      />
      {generalInputs ? (
        <div className={styles.followUp}>
          <ContributionsForm
            startDate={generalInputs.startDate}
            endDate={generalInputs.endDate}
            onValidSubmit={setContributions}
          />
        </div>
      ) : null}
      {generalInputs && contributions !== null ? (
        <div className={styles.followUp}>
          <TesouroRatesForm
            startDate={generalInputs.startDate}
            endDate={generalInputs.endDate}
          />
        </div>
      ) : null}
    </section>
  );
}
