import { render, screen } from '@testing-library/react';
import { SimulationResultSummary } from '@/components/simulation/SimulationResultSummary';
import type { SimulationResultResponse } from '@/types/simulationApi';

const result: SimulationResultResponse = {
  initialAmount: 10000,
  totalAdditionalContributions: 1000,
  totalInvested: 11000,
  grossAmount: 12000,
  grossReturnPercentage: 0.0909,
  costs: 0,
  incomeTax: 150,
  iof: 0,
  netAmount: 11850,
  netReturnPercentage: 0.0773,
  totalNetYield: 850,
  netAmountInflationAdjusted: 11500,
  contributionDetails: [],
};

describe('SimulationResultSummary', () => {
  it('renders the ERS §19 summary labels', () => {
    render(<SimulationResultSummary result={result} />);

    expect(
      screen.getByRole('heading', { name: /resultado da simulação/i }),
    ).toBeInTheDocument();
    expect(screen.getByText('Valor líquido')).toBeInTheDocument();
    expect(screen.getByText('Lucro líquido')).toBeInTheDocument();
    expect(
      screen.getByText('Valor líquido ajustado pela inflação'),
    ).toBeInTheDocument();
  });
});
