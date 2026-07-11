import { render, screen } from '@testing-library/react';
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

    await user.click(screen.getByRole('button', { name: 'Validar taxas' }));

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
    await user.click(screen.getByRole('button', { name: 'Validar taxas' }));

    expect(onValidSubmit).toHaveBeenCalledWith({
      profitabilityPercentage: '120',
      cdi: {
        mode: 'single',
        singleRate: '15',
        rates: [],
      },
    });
    expect(screen.getByText(/taxas válidas/i)).toBeInTheDocument();
  });

  it('switches to year-by-year mode and generates years for the period', async () => {
    const user = userEvent.setup();
    const { onValidSubmit } = renderForm();

    await user.type(screen.getByLabelText('Rentabilidade (% do CDI)'), '110');
    await user.type(screen.getByLabelText('Taxa anual (%)'), '14');
    await user.click(screen.getByLabelText('Ano a ano'));

    expect(screen.getByLabelText('Taxa de CDI anual em 2026')).toHaveValue(
      '14',
    );
    expect(screen.getByLabelText('Taxa de CDI anual em 2027')).toHaveValue(
      '14',
    );

    await user.clear(screen.getByLabelText('Taxa de CDI anual em 2027'));
    await user.type(screen.getByLabelText('Taxa de CDI anual em 2027'), '12');
    await user.click(screen.getByRole('button', { name: 'Validar taxas' }));

    expect(onValidSubmit).toHaveBeenCalledWith({
      profitabilityPercentage: '110',
      cdi: {
        mode: 'perYear',
        singleRate: '14',
        rates: [
          { year: 2026, rate: '14' },
          { year: 2027, rate: '12' },
        ],
      },
    });
  });
});
