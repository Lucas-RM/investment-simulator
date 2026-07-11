import { Navigate } from 'react-router-dom';
import { SimulationResultSummary } from '@/components/simulation/SimulationResultSummary';
import { SimulatorStepLayout } from '@/pages/SimulatorStepLayout';
import { simulationStepPaths } from '@/routes/simulationSteps';
import { InvestmentType } from '@/types/investment';
import { loadSimulationResult } from '@/utils/simulationResultStorage';

export type SimulationResultPageProps = {
  investmentType: InvestmentType;
};

export function SimulationResultPage({
  investmentType,
}: SimulationResultPageProps) {
  const stepPaths = simulationStepPaths(investmentType);
  const result = loadSimulationResult(investmentType);

  if (!result) {
    return <Navigate to={stepPaths.rates} replace />;
  }

  const title =
    investmentType === InvestmentType.Cdb
      ? 'Resultado — CDB'
      : 'Resultado — Tesouro Selic';

  return (
    <SimulatorStepLayout
      title={title}
      description="Resumo final da simulação (ERS §19)."
      backTo={stepPaths.rates}
      backLabel="Voltar às taxas"
    >
      <SimulationResultSummary result={result} showHeading={false} />
    </SimulatorStepLayout>
  );
}
