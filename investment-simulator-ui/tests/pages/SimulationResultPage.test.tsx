import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { AppRoutes } from '@/routes/AppRoutes';
import { paths } from '@/routes/paths';
import { createEmptyCdbDraft } from '@/types/simulationDraft';
import { InvestmentType } from '@/types/investment';
import type { SimulationResultResponse } from '@/types/simulationApi';
import {
  clearSimulationDraft,
  saveSimulationDraft,
} from '@/utils/simulationDraftStorage';
import {
  clearSimulationResult,
  saveSimulationResult,
} from '@/utils/simulationResultStorage';

const sampleResult: SimulationResultResponse = {
  startDate: '2026-01-02',
  endDate: '2026-12-31',
  initialAmount: 10000,
  totalAdditionalContributions: 0,
  totalInvested: 10000,
  grossAmount: 10100,
  grossReturnPercentage: 0.01,
  totalGrossYield: 100,
  costs: 0,
  incomeTax: 15,
  iof: 0,
  netAmount: 10085,
  netReturnPercentage: 0.0085,
  totalNetYield: 85,
  netAmountInflationAdjusted: 10085,
  contributionDetails: [],
};

function renderAt(path: string) {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <AppRoutes />
    </MemoryRouter>,
  );
}

function seedCdbDraftReadyForRates() {
  const draft = createEmptyCdbDraft();
  draft.generalInputs = {
    initialAmount: '10000',
    startDate: '2026-01-01',
    endDate: '2027-01-01',
  };
  draft.contributionsConfirmed = true;
  saveSimulationDraft(draft);
}

describe('SimulationResultPage', () => {
  beforeEach(() => {
    clearSimulationResult(InvestmentType.Cdb);
    clearSimulationResult(InvestmentType.TesouroSelic);
    clearSimulationDraft(InvestmentType.Cdb);
  });

  it('shows the stored CDB result on the result route', () => {
    saveSimulationResult(InvestmentType.Cdb, sampleResult);
    renderAt(paths.cdbResult);

    expect(
      screen.getByRole('heading', { name: 'Resultado — CDB' }),
    ).toBeInTheDocument();
    expect(
      screen.getByLabelText('Indicadores principais'),
    ).toBeInTheDocument();
    expect(screen.getByLabelText('Período da simulação')).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: /voltar às taxas/i }),
    ).toHaveAttribute('href', paths.cdbRates);
  });

  it('redirects to rates when there is no stored result', () => {
    seedCdbDraftReadyForRates();
    renderAt(paths.cdbResult);

    expect(
      screen.getByRole('heading', { name: 'Simulação CDB' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('group', { name: 'Taxas — CDB' }),
    ).toBeInTheDocument();
  });
});
