import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { CdbRatesForm } from '@/components/simulation/CdbRatesForm';

const startDate = '2026-01-01';
const endDate = '2027-06-01';

function renderForm(onValidSubmit = vi.fn()) {
  render(
    <CdbRatesForm
      startDate={startDate}
      endDate={endDate}
      onValidSubmit={onValidSubmit}
    />,
  );

  return { onValidSubmit };
}

describe('CdbRatesForm', () => {
  it('renders profitability and CDI schedule fields', () => {
    renderForm();

    expect(
      screen.getByRole('group', { name: 'Taxas — CDB' }),
    ).toBeInTheDocument();
    expect(
      screen.getByLabelText('Rentabilidade (% do CDI)'),
    ).toBeInTheDocument();
    expect(screen.getByLabelText('Taxa anual (%)')).toBeInTheDocument();
    expect(screen.getByLabelText('Taxa única')).toBeInTheDocument();
    expect(screen.getByLabelText('Ano a ano')).toBeInTheDocument();
  });

  it('shows validation errors for empty required fields', async () => {
    const user = userEvent.setup();
    const { onValidSubmit } = renderForm();

    await user.click(screen.getByRole('button', { name: 'Simular' }));

    const alerts = screen.getAllByRole('alert');
    expect(alerts.map((node) => node.textContent)).toEqual(
      expect.arrayContaining([
        expect.stringMatching(/informe a rentabilidade/i),
        expect.stringMatching(/informe a taxa de cdi/i),
      ]),
    );
    expect(onValidSubmit).not.toHaveBeenCalled();
  });

  it('submits valid single-rate CDB values', async () => {
    const user = userEvent.setup();
    const { onValidSubmit } = renderForm();

    await user.type(screen.getByLabelText('Rentabilidade (% do CDI)'), '120');
    await user.type(screen.getByLabelText('Taxa anual (%)'), '15');
    await user.click(screen.getByRole('button', { name: 'Simular' }));

    expect(onValidSubmit).toHaveBeenCalledWith({
      profitabilityPercentage: '120',
      cdi: {
        mode: 'single',
        singleRate: '15',
        rates: [],
      },
    });
  });

  it('opens the per-year modal and applies rates for the period', async () => {
    const user = userEvent.setup();
    const { onValidSubmit } = renderForm();

    await user.type(screen.getByLabelText('Rentabilidade (% do CDI)'), '110');
    await user.type(screen.getByLabelText('Taxa anual (%)'), '14');
    await user.click(screen.getByLabelText('Ano a ano'));

    expect(
      screen.getByRole('button', { name: 'Editar taxas ano a ano' }),
    ).toBeInTheDocument();
    expect(
      screen.getByText(/2 de 2 ano\(s\) com taxa informada/i),
    ).toBeInTheDocument();

    await user.click(
      screen.getByRole('button', { name: 'Editar taxas ano a ano' }),
    );

    const dialog = screen.getByRole('dialog');
    expect(
      within(dialog).getByRole('heading', { name: /cdi anual — ano a ano/i }),
    ).toBeInTheDocument();

    const rate2026 = within(dialog).getByLabelText('Taxa de CDI anual em 2026');
    const rate2027 = within(dialog).getByLabelText('Taxa de CDI anual em 2027');
    expect(rate2026).toHaveValue('14');
    expect(rate2027).toHaveValue('14');

    await user.clear(rate2027);
    await user.type(rate2027, '12');
    await user.click(
      within(dialog).getByRole('button', { name: 'Aplicar taxas' }),
    );

    expect(
      screen.getByText(/2 de 2 ano\(s\) com taxa informada/i),
    ).toBeInTheDocument();
    expect(screen.getByText('12%')).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Simular' }));

    expect(onValidSubmit).toHaveBeenCalledWith({
      profitabilityPercentage: '110',
      cdi: {
        mode: 'perYear',
        singleRate: '',
        rates: [
          { year: 2026, rate: '14' },
          { year: 2027, rate: '12' },
        ],
      },
    });
  });

  it('shows submit error and disables the button while submitting', async () => {
    const user = userEvent.setup();
    let resolveSubmit: (() => void) | undefined;
    const onValidSubmit = vi.fn(
      () =>
        new Promise<void>((resolve) => {
          resolveSubmit = resolve;
        }),
    );

    const { rerender } = render(
      <CdbRatesForm
        startDate={startDate}
        endDate={endDate}
        onValidSubmit={onValidSubmit}
        isSubmitting={false}
        submitError={null}
      />,
    );

    await user.type(screen.getByLabelText('Rentabilidade (% do CDI)'), '100');
    await user.type(screen.getByLabelText('Taxa anual (%)'), '14');
    await user.click(screen.getByRole('button', { name: 'Simular' }));

    expect(onValidSubmit).toHaveBeenCalled();

    rerender(
      <CdbRatesForm
        startDate={startDate}
        endDate={endDate}
        onValidSubmit={onValidSubmit}
        isSubmitting
        submitError={null}
      />,
    );

    expect(screen.getByRole('button', { name: 'Simulando…' })).toBeDisabled();
    expect(screen.getByText(/calculando a simulação/i)).toBeInTheDocument();

    rerender(
      <CdbRatesForm
        startDate={startDate}
        endDate={endDate}
        onValidSubmit={onValidSubmit}
        isSubmitting={false}
        submitError="Falha na API"
      />,
    );

    expect(screen.getByRole('alert')).toHaveTextContent('Falha na API');
    resolveSubmit?.();
  });
});
