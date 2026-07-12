import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ContributionDetailsModal } from '@/components/simulation/ContributionDetailsModal';
import type { ContributionDetailResponse } from '@/types/simulationApi';

const details: ContributionDetailResponse[] = [
  {
    date: '2026-07-06',
    amount: 900,
    grossBalance: 972.53,
    grossYield: 72.53,
    calendarDaysInvested: 178,
    businessDaysInvested: 123,
    incomeTax: 16.32,
    iof: 0,
  },
  {
    date: '2026-12-07',
    amount: 900,
    grossBalance: 909.69,
    grossYield: 9.69,
    calendarDaysInvested: 24,
    businessDaysInvested: 17,
    incomeTax: 1.74,
    iof: 1.94,
  },
];

describe('ContributionDetailsModal', () => {
  it('renders Portuguese column headers and contribution rows', () => {
    render(
      <ContributionDetailsModal open details={details} onClose={vi.fn()} />,
    );

    const dialog = screen.getByRole('dialog');
    expect(
      within(dialog).getByRole('heading', { name: /detalhamento por aporte/i }),
    ).toBeInTheDocument();

    for (const header of [
      'Data',
      'Valor',
      'Saldo bruto',
      'Rendimento bruto',
      'Dias corridos',
      'Dias úteis',
      'IR',
      'IOF',
    ]) {
      expect(within(dialog).getByText(header)).toBeInTheDocument();
    }

    expect(within(dialog).getByText('06/07/2026')).toBeInTheDocument();
    expect(within(dialog).getByText('178')).toBeInTheDocument();
    expect(within(dialog).getByText('123')).toBeInTheDocument();
  });

  it('calls onClose when Fechar is clicked', async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    render(
      <ContributionDetailsModal open details={details} onClose={onClose} />,
    );

    await user.click(screen.getByRole('button', { name: 'Fechar' }));
    expect(onClose).toHaveBeenCalledOnce();
  });
});
