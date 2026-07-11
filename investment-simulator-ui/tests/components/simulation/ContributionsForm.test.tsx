import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ContributionsForm } from '@/components/simulation/ContributionsForm';

const startDate = '2026-01-01';
const endDate = '2027-01-01';

function renderForm(
  onValidSubmit = vi.fn(),
  defaultContributions?: Array<{ date: string; amount: string }>,
  initialAmount = '10000',
) {
  render(
    <ContributionsForm
      startDate={startDate}
      endDate={endDate}
      initialAmount={initialAmount}
      defaultContributions={defaultContributions}
      onValidSubmit={onValidSubmit}
    />,
  );

  return { onValidSubmit };
}

describe('ContributionsForm', () => {
  it('renders the contributions section and allows adding rows', async () => {
    const user = userEvent.setup();
    renderForm();

    expect(
      screen.getByRole('group', { name: 'Aportes adicionais' }),
    ).toBeInTheDocument();
    expect(
      screen.getByText(/nenhum aporte adicional cadastrado/i),
    ).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Adicionar aporte' }));

    expect(screen.getByLabelText('Data do aporte 1')).toBeInTheDocument();
    expect(screen.getByLabelText('Valor do aporte 1')).toBeInTheDocument();
  });

  it('shows inline validation errors for invalid rows', async () => {
    const user = userEvent.setup();
    const { onValidSubmit } = renderForm();

    await user.click(screen.getByRole('button', { name: 'Adicionar aporte' }));
    await user.click(screen.getByRole('button', { name: 'Continuar' }));

    const alerts = screen.getAllByRole('alert');
    expect(alerts.map((node) => node.textContent)).toEqual(
      expect.arrayContaining([
        expect.stringMatching(/informe a data/i),
        expect.stringMatching(/informe o valor/i),
      ]),
    );
    expect(onValidSubmit).not.toHaveBeenCalled();
  });

  it('submits a valid contribution list', async () => {
    const user = userEvent.setup();
    const { onValidSubmit } = renderForm();

    await user.click(screen.getByRole('button', { name: 'Adicionar aporte' }));
    await user.type(screen.getByLabelText('Data do aporte 1'), '2026-03-15');
    await user.type(screen.getByLabelText('Valor do aporte 1'), '1200.50');
    await user.click(screen.getByRole('button', { name: 'Continuar' }));

    expect(onValidSubmit).toHaveBeenCalledWith([
      { date: '2026-03-15', amount: '1200.50' },
    ]);
  });

  it('allows submitting with zero additional contributions', async () => {
    const user = userEvent.setup();
    const { onValidSubmit } = renderForm();

    await user.click(screen.getByRole('button', { name: 'Continuar' }));

    expect(onValidSubmit).toHaveBeenCalledWith([]);
  });

  it('blocks continue when initial amount is zero and there are no contributions', async () => {
    const user = userEvent.setup();
    const { onValidSubmit } = renderForm(vi.fn(), undefined, '0');

    await user.click(screen.getByRole('button', { name: 'Continuar' }));

    expect(screen.getByRole('alert').textContent).toMatch(
      /valor inicial maior que zero ou pelo menos um aporte/i,
    );
    expect(onValidSubmit).not.toHaveBeenCalled();
  });

  it('allows zero initial amount when there is at least one contribution', async () => {
    const user = userEvent.setup();
    const { onValidSubmit } = renderForm(vi.fn(), undefined, '0');

    await user.click(screen.getByRole('button', { name: 'Adicionar aporte' }));
    await user.type(screen.getByLabelText('Data do aporte 1'), '2026-03-15');
    await user.type(screen.getByLabelText('Valor do aporte 1'), '500');
    await user.click(screen.getByRole('button', { name: 'Continuar' }));

    expect(onValidSubmit).toHaveBeenCalledWith([
      { date: '2026-03-15', amount: '500' },
    ]);
  });

  it('adds and removes rows dynamically', async () => {
    const user = userEvent.setup();
    renderForm();

    await user.click(screen.getByRole('button', { name: 'Adicionar aporte' }));
    await user.click(screen.getByRole('button', { name: 'Adicionar aporte' }));

    expect(screen.getByLabelText('Data do aporte 1')).toBeInTheDocument();
    expect(screen.getByLabelText('Data do aporte 2')).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Remover aporte 1' }));

    expect(screen.queryByLabelText('Data do aporte 2')).not.toBeInTheDocument();
    expect(screen.getByLabelText('Data do aporte 1')).toBeInTheDocument();

    const table = screen.getByRole('table');
    expect(within(table).getAllByRole('row')).toHaveLength(2); // header + 1
  });

  it('rejects a contribution after the redemption date', async () => {
    const user = userEvent.setup();
    const { onValidSubmit } = renderForm();

    await user.click(screen.getByRole('button', { name: 'Adicionar aporte' }));
    await user.type(screen.getByLabelText('Data do aporte 1'), '2027-02-01');
    await user.type(screen.getByLabelText('Valor do aporte 1'), '500');
    await user.click(screen.getByRole('button', { name: 'Continuar' }));

    expect(screen.getByRole('alert').textContent).toMatch(
      /posterior à data de resgate/i,
    );
    expect(onValidSubmit).not.toHaveBeenCalled();
  });

  it('generates recurring first-Monday contributions from the modal', async () => {
    const user = userEvent.setup();
    renderForm();

    await user.click(
      screen.getByRole('button', { name: 'Gerar aportes recorrentes' }),
    );

    expect(
      screen.getByRole('heading', { name: 'Gerar aportes recorrentes' }),
    ).toBeInTheDocument();

    await user.selectOptions(screen.getByLabelText('Dia útil'), 'monday');
    await user.type(screen.getByLabelText('Valor de cada aporte (R$)'), '500');
    await user.click(screen.getByRole('button', { name: 'Gerar aportes' }));

    expect(screen.getByLabelText('Data do aporte 1')).toHaveValue('2026-01-05');
    expect(screen.getByLabelText('Valor do aporte 1')).toHaveValue('500');
    expect(screen.getByLabelText('Data do aporte 2')).toHaveValue('2026-02-02');
    expect(
      screen.queryByRole('heading', { name: 'Gerar aportes recorrentes' }),
    ).not.toBeInTheDocument();
  });

  it('generates recurring first-Friday contributions from the modal', async () => {
    const user = userEvent.setup();
    renderForm();

    await user.click(
      screen.getByRole('button', { name: 'Gerar aportes recorrentes' }),
    );
    await user.selectOptions(screen.getByLabelText('Dia útil'), 'friday');
    await user.type(screen.getByLabelText('Valor de cada aporte (R$)'), '900');
    await user.click(screen.getByRole('button', { name: 'Gerar aportes' }));

    expect(screen.getByLabelText('Data do aporte 1')).toHaveValue('2026-01-02');
    expect(screen.getByLabelText('Valor do aporte 1')).toHaveValue('900');
  });
});
