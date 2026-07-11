import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { AppRoutes } from '@/routes/AppRoutes';
import { paths } from '@/routes/paths';
import { clearSimulationDraft } from '@/utils/simulationDraftStorage';
import { InvestmentType } from '@/types/investment';

function renderAt(path: string) {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <AppRoutes />
    </MemoryRouter>,
  );
}

describe('simulation wizard steps', () => {
  beforeEach(() => {
    clearSimulationDraft(InvestmentType.Cdb);
    clearSimulationDraft(InvestmentType.TesouroSelic);
  });

  it('shows only general inputs on the CDB entry page', () => {
    renderAt(paths.cdb);

    expect(
      screen.getByRole('group', { name: 'Entradas gerais' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('group', { name: 'Aportes adicionais' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('group', { name: 'Taxas — CDB' }),
    ).not.toBeInTheDocument();
  });

  it('navigates to contributions after continuing from general inputs', async () => {
    const user = userEvent.setup();
    renderAt(paths.cdb);

    const amountInput = screen.getByLabelText('Valor inicial (R$)');
    await user.clear(amountInput);
    await user.type(amountInput, '10000');
    await user.type(screen.getByLabelText('Data inicial'), '2026-01-01');
    await user.type(screen.getByLabelText('Data de resgate'), '2027-01-01');
    await user.click(screen.getByRole('button', { name: 'Continuar' }));

    expect(
      screen.getByRole('group', { name: 'Aportes adicionais' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('group', { name: 'Entradas gerais' }),
    ).not.toBeInTheDocument();
    expect(screen.getByText(/etapa 2 de 3/i)).toBeInTheDocument();
  });

  it('restores general inputs from localStorage after remount', async () => {
    const user = userEvent.setup();
    const { unmount } = renderAt(paths.cdb);

    const amountInput = screen.getByLabelText('Valor inicial (R$)');
    await user.clear(amountInput);
    await user.type(amountInput, '2500.75');
    await user.type(screen.getByLabelText('Data inicial'), '2026-02-10');
    await user.type(screen.getByLabelText('Data de resgate'), '2028-02-10');

    unmount();
    renderAt(paths.cdb);

    expect(screen.getByLabelText('Valor inicial (R$)')).toHaveValue('2500.75');
    expect(screen.getByLabelText('Data inicial')).toHaveValue('2026-02-10');
    expect(screen.getByLabelText('Data de resgate')).toHaveValue('2028-02-10');
  });

  it('shows a back button to the previous step on contributions', async () => {
    const user = userEvent.setup();
    renderAt(paths.cdb);

    const amountInput = screen.getByLabelText('Valor inicial (R$)');
    await user.clear(amountInput);
    await user.type(amountInput, '10000');
    await user.type(screen.getByLabelText('Data inicial'), '2026-01-01');
    await user.type(screen.getByLabelText('Data de resgate'), '2027-01-01');
    await user.click(screen.getByRole('button', { name: 'Continuar' }));

    const back = screen.getByRole('link', { name: /voltar/i });
    expect(back).toBeInTheDocument();
    await user.click(back);

    expect(
      screen.getByRole('group', { name: 'Entradas gerais' }),
    ).toBeInTheDocument();
  });

  it('shows a back button to home on the general inputs step', () => {
    renderAt(paths.cdb);

    expect(
      screen.getByRole('link', { name: /voltar ao início/i }),
    ).toHaveAttribute('href', paths.home);
  });

  it('redirects contributions step to general when draft is incomplete', () => {
    renderAt(paths.cdbContributions);

    expect(
      screen.getByRole('group', { name: 'Entradas gerais' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('group', { name: 'Aportes adicionais' }),
    ).not.toBeInTheDocument();
  });
});
