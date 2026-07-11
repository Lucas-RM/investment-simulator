import { render, screen, within } from '@testing-library/react';
import { SimulationResultSummary } from '@/components/simulation/SimulationResultSummary';
import type { SimulationResultResponse } from '@/types/simulationApi';

const result: SimulationResultResponse = {
  startDate: '2026-01-02',
  endDate: '2027-01-02',
  initialAmount: 10000,
  totalAdditionalContributions: 1000,
  totalInvested: 11000,
  grossAmount: 12000,
  grossReturnPercentage: 0.0909,
  totalGrossYield: 1000,
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
  it('renders period, highlights and grouped summary labels', () => {
    render(<SimulationResultSummary result={result} />);

    expect(
      screen.getByRole('heading', { name: /resultado da simulação/i }),
    ).toBeInTheDocument();

    const period = screen.getByLabelText('Período da simulação');
    expect(within(period).getByText('Data inicial')).toBeInTheDocument();
    expect(within(period).getByText('Data de resgate')).toBeInTheDocument();

    const highlights = screen.getByLabelText('Indicadores principais');
    expect(within(highlights).getByText('Valor líquido')).toBeInTheDocument();
    expect(within(highlights).getByText('Lucro líquido')).toBeInTheDocument();
    expect(within(highlights).getByText('Lucro bruto')).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Investimento' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('heading', { name: 'Resultado final' }),
    ).toBeInTheDocument();
    expect(
      screen.getByText('Valor líquido ajustado pela inflação'),
    ).toBeInTheDocument();
  });
});
