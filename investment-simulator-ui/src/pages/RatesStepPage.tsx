import { Navigate } from 'react-router-dom';
import { CdbRatesForm } from '@/components/simulation/CdbRatesForm';
import { SimulationResultSummary } from '@/components/simulation/SimulationResultSummary';
import { TesouroRatesForm } from '@/components/simulation/TesouroRatesForm';
import { useRunSimulation } from '@/hooks/useRunSimulation';
import { useSimulationDraft } from '@/hooks/useSimulationDraft';
import { SimulatorStepLayout } from '@/pages/SimulatorStepLayout';
import { simulationStepPaths } from '@/routes/simulationSteps';
import type { GeneralInputs } from '@/types/generalInputs';
import { InvestmentType } from '@/types/investment';
import type { CdbRatesInput, TesouroRatesInput } from '@/types/rates';
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
  const { run, reset, result, error, isLoading } = useRunSimulation();
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

  async function handleCdbSubmit(rates: CdbRatesInput) {
    updateCdbRates(rates);
    if (!isCdbDraft(draft)) {
      return;
    }
    await run({ ...draft, rates });
  }

  async function handleTesouroSubmit(rates: TesouroRatesInput) {
    updateTesouroRates(rates);
    if (!isTesouroDraft(draft)) {
      return;
    }
    await run({ ...draft, rates });
  }

  function handleRatesChangeCdb(rates: CdbRatesInput) {
    updateCdbRates(rates);
    reset();
  }

  function handleRatesChangeTesouro(rates: TesouroRatesInput) {
    updateTesouroRates(rates);
    reset();
  }

  return (
    <SimulatorStepLayout
      title={title}
      description="Etapa 3 de 3 — Informe as taxas e execute a simulação."
      backTo={stepPaths.contributions}
      backLabel="Voltar"
    >
      {investmentType === InvestmentType.Cdb && isCdbDraft(draft) ? (
        <CdbRatesForm
          startDate={draft.generalInputs.startDate}
          endDate={draft.generalInputs.endDate}
          defaultValues={draft.rates ?? undefined}
          onValuesChange={handleRatesChangeCdb}
          onValidSubmit={handleCdbSubmit}
          isSubmitting={isLoading}
          submitError={error}
        />
      ) : null}
      {investmentType === InvestmentType.TesouroSelic &&
      isTesouroDraft(draft) ? (
        <TesouroRatesForm
          startDate={draft.generalInputs.startDate}
          endDate={draft.generalInputs.endDate}
          defaultValues={draft.rates ?? undefined}
          onValuesChange={handleRatesChangeTesouro}
          onValidSubmit={handleTesouroSubmit}
          isSubmitting={isLoading}
          submitError={error}
        />
      ) : null}

      {result ? <SimulationResultSummary result={result} /> : null}
    </SimulatorStepLayout>
  );
}
