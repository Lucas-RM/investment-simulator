import { Navigate, useNavigate } from 'react-router-dom';
import { ContributionsForm } from '@/components/simulation/ContributionsForm';
import { useSimulationDraft } from '@/hooks/useSimulationDraft';
import { SimulatorStepLayout } from '@/pages/SimulatorStepLayout';
import { simulationStepPaths } from '@/routes/simulationSteps';
import type { ContributionInput } from '@/types/contribution';
import type { GeneralInputs } from '@/types/generalInputs';
import { InvestmentType } from '@/types/investment';
import {
  hasGeneralInputsErrors,
  validateGeneralInputs,
} from '@/utils/validateGeneralInputs';

export type ContributionsStepPageProps = {
  investmentType: InvestmentType;
};

function toGeneralInputs(
  investmentType: InvestmentType,
  draft: {
    initialAmount: string;
    startDate: string;
    endDate: string;
  },
): GeneralInputs {
  return {
    investmentType,
    initialAmount: draft.initialAmount,
    startDate: draft.startDate,
    endDate: draft.endDate,
  };
}

export function ContributionsStepPage({
  investmentType,
}: ContributionsStepPageProps) {
  const navigate = useNavigate();
  const { draft, updateDraft } = useSimulationDraft(investmentType);
  const stepPaths = simulationStepPaths(investmentType);

  const generalInputs = toGeneralInputs(investmentType, draft.generalInputs);
  if (hasGeneralInputsErrors(validateGeneralInputs(generalInputs))) {
    return <Navigate to={stepPaths.general} replace />;
  }

  const title =
    investmentType === InvestmentType.Cdb
      ? 'Simulação CDB'
      : 'Simulação Tesouro Selic';

  function handleContributionsChange(contributions: ContributionInput[]) {
    updateDraft({
      contributions,
      contributionsConfirmed: false,
    });
  }

  function handleValidSubmit(contributions: ContributionInput[]) {
    updateDraft({
      contributions,
      contributionsConfirmed: true,
    });
    navigate(stepPaths.rates);
  }

  return (
    <SimulatorStepLayout
      title={title}
      description="Etapa 2 de 3 — Cadastre os aportes adicionais (ou continue sem nenhum)."
      backTo={stepPaths.general}
      backLabel="Voltar às entradas gerais"
    >
      <ContributionsForm
        startDate={draft.generalInputs.startDate}
        endDate={draft.generalInputs.endDate}
        defaultContributions={draft.contributions}
        onContributionsChange={handleContributionsChange}
        onValidSubmit={handleValidSubmit}
      />
    </SimulatorStepLayout>
  );
}
