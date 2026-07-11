import { Navigate } from 'react-router-dom';
import { CdbRatesForm } from '@/components/simulation/CdbRatesForm';
import { TesouroRatesForm } from '@/components/simulation/TesouroRatesForm';
import { useSimulationDraft } from '@/hooks/useSimulationDraft';
import { SimulatorStepLayout } from '@/pages/SimulatorStepLayout';
import { simulationStepPaths } from '@/routes/simulationSteps';
import type { GeneralInputs } from '@/types/generalInputs';
import { InvestmentType } from '@/types/investment';
import {
  hasGeneralInputsErrors,
  validateGeneralInputs,
} from '@/utils/validateGeneralInputs';
import { isCdbDraft, isTesouroDraft } from '@/utils/simulationDraftStorage';

export type RatesStepPageProps = {
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

export function RatesStepPage({ investmentType }: RatesStepPageProps) {
  const { draft, updateCdbRates, updateTesouroRates } =
    useSimulationDraft(investmentType);
  const stepPaths = simulationStepPaths(investmentType);

  const generalInputs = toGeneralInputs(investmentType, draft.generalInputs);
  if (hasGeneralInputsErrors(validateGeneralInputs(generalInputs))) {
    return <Navigate to={stepPaths.general} replace />;
  }

  if (!draft.contributionsConfirmed) {
    return <Navigate to={stepPaths.contributions} replace />;
  }

  const title =
    investmentType === InvestmentType.Cdb
      ? 'Simulação CDB'
      : 'Simulação Tesouro Selic';

  return (
    <SimulatorStepLayout
      title={title}
      description="Etapa 3 de 3 — Informe as taxas da simulação."
      backTo={stepPaths.contributions}
      backLabel="Voltar aos aportes"
    >
      {investmentType === InvestmentType.Cdb && isCdbDraft(draft) ? (
        <CdbRatesForm
          startDate={draft.generalInputs.startDate}
          endDate={draft.generalInputs.endDate}
          defaultValues={draft.rates ?? undefined}
          onValuesChange={updateCdbRates}
          onValidSubmit={updateCdbRates}
        />
      ) : null}
      {investmentType === InvestmentType.TesouroSelic &&
      isTesouroDraft(draft) ? (
        <TesouroRatesForm
          startDate={draft.generalInputs.startDate}
          endDate={draft.generalInputs.endDate}
          defaultValues={draft.rates ?? undefined}
          onValuesChange={updateTesouroRates}
          onValidSubmit={updateTesouroRates}
        />
      ) : null}
    </SimulatorStepLayout>
  );
}
