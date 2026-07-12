import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
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
  contributionDetails: [
    {
      date: '2026-01-02',
      amount: 10000,
      grossBalance: 11000,
      grossYield: 1000,
      calendarDaysInvested: 365,
      businessDaysInvested: 252,
      incomeTax: 150,
      iof: 0,
    },
    {
      date: '2026-06-01',
      amount: 1000,
      grossBalance: 1050,
      grossYield: 50,
      calendarDaysInvested: 215,
      businessDaysInvested: 148,
      incomeTax: 11.25,
      iof: 0,
    },
  ],
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
    expect(within(highlights).getByText('Total investido')).toBeInTheDocument();

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

  it('opens contribution details table from Aportes adicionais', async () => {
    const user = userEvent.setup();
    render(<SimulationResultSummary result={result} />);

    await user.click(screen.getByRole('button', { name: 'Ver detalhamento' }));

    const dialog = screen.getByRole('dialog');
    expect(
      within(dialog).getByRole('heading', { name: /detalhamento por aporte/i }),
    ).toBeInTheDocument();
    expect(within(dialog).getByText('Data')).toBeInTheDocument();
    expect(within(dialog).getByText('Saldo bruto')).toBeInTheDocument();
    expect(within(dialog).getByText('Rendimento bruto')).toBeInTheDocument();
    expect(within(dialog).getByText('Dias corridos')).toBeInTheDocument();
    expect(within(dialog).getByText('Dias úteis')).toBeInTheDocument();
    expect(within(dialog).getByText('02/01/2026')).toBeInTheDocument();
    expect(within(dialog).getAllByText(/R\$/).length).toBeGreaterThan(0);

    await user.click(within(dialog).getByRole('button', { name: 'Fechar' }));
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
  });
});
