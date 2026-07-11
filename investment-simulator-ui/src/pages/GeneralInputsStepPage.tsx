import { useNavigate } from 'react-router-dom';
import { GeneralInputsForm } from '@/components/simulation/GeneralInputsForm';
import { useSimulationDraft } from '@/hooks/useSimulationDraft';
import { SimulatorStepLayout } from '@/pages/SimulatorStepLayout';
import { paths } from '@/routes/paths';
import { simulationStepPaths } from '@/routes/simulationSteps';
import type { GeneralInputs } from '@/types/generalInputs';
import { InvestmentType } from '@/types/investment';

export type GeneralInputsStepPageProps = {
  investmentType: InvestmentType;
};

export function GeneralInputsStepPage({
  investmentType,
}: GeneralInputsStepPageProps) {
  const navigate = useNavigate();
  const { draft, updateDraft } = useSimulationDraft(investmentType);
  const stepPaths = simulationStepPaths(investmentType);

  const title =
    investmentType === InvestmentType.Cdb
      ? 'Simulação CDB'
      : 'Simulação Tesouro Selic';

  function handleValuesChange(values: GeneralInputs) {
    updateDraft({
      generalInputs: {
        initialAmount: values.initialAmount,
        startDate: values.startDate,
        endDate: values.endDate,
      },
    });
  }

  function handleValidSubmit(values: GeneralInputs) {
    updateDraft({
      generalInputs: {
        initialAmount: values.initialAmount,
        startDate: values.startDate,
        endDate: values.endDate,
      },
      contributionsConfirmed: false,
    });
    navigate(stepPaths.contributions);
  }

  return (
    <SimulatorStepLayout
      title={title}
      description="Etapa 1 de 3 — Informe o valor inicial e o período da simulação."
      backTo={paths.home}
      backLabel="Voltar ao início"
    >
      <GeneralInputsForm
        defaultInvestmentType={investmentType}
        defaultValues={draft.generalInputs}
        onValuesChange={handleValuesChange}
        onValidSubmit={handleValidSubmit}
      />
    </SimulatorStepLayout>
  );
}
