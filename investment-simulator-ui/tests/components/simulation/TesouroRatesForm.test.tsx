import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { TesouroRatesForm } from '@/components/simulation/TesouroRatesForm';

const startDate = '2026-01-01';
const endDate = '2026-12-31';

function renderForm(onValidSubmit = vi.fn()) {
  render(
    <TesouroRatesForm
      startDate={startDate}
      endDate={endDate}
      onValidSubmit={onValidSubmit}
    />,
  );

  return { onValidSubmit };
}

describe('TesouroRatesForm', () => {
  it('renders Selic, ágio, B3 and IPCA schedules', () => {
    renderForm();

    expect(
      screen.getByRole('group', { name: 'Taxas — Tesouro Selic' }),
    ).toBeInTheDocument();
    expect(screen.getByText('Selic Over anual')).toBeInTheDocument();
    expect(screen.getByText('Ágio / deságio anual')).toBeInTheDocument();
    expect(screen.getByText('Taxa de custódia B3')).toBeInTheDocument();
    expect(screen.getByText('IPCA anual')).toBeInTheDocument();
  });

  it('shows validation errors when schedules are empty', async () => {
    const user = userEvent.setup();
    const { onValidSubmit } = renderForm();

    await user.click(screen.getByRole('button', { name: 'Validar taxas' }));

    const alerts = screen.getAllByRole('alert');
    expect(alerts.length).toBeGreaterThanOrEqual(4);
    expect(onValidSubmit).not.toHaveBeenCalled();
  });

  it('submits valid single-rate Tesouro values including negative ágio', async () => {
    const user = userEvent.setup();
    const { onValidSubmit } = renderForm();

    const rateInputs = screen.getAllByLabelText('Taxa anual (%)');
    expect(rateInputs).toHaveLength(4);

    await user.type(rateInputs[0], '14.15');
    await user.type(rateInputs[1], '-0.05');
    await user.type(rateInputs[2], '0.2');
    await user.type(rateInputs[3], '4.5');
    await user.click(screen.getByRole('button', { name: 'Validar taxas' }));

    expect(onValidSubmit).toHaveBeenCalledWith({
      selic: { mode: 'single', singleRate: '14.15', rates: [] },
      agio: { mode: 'single', singleRate: '-0.05', rates: [] },
      b3Custody: { mode: 'single', singleRate: '0.2', rates: [] },
      ipca: { mode: 'single', singleRate: '4.5', rates: [] },
    });
    expect(screen.getByText(/taxas válidas/i)).toBeInTheDocument();
  });

  it('can switch one schedule to year-by-year independently', async () => {
    const user = userEvent.setup();
    renderForm();

    const perYearRadios = screen.getAllByLabelText('Ano a ano');
    await user.click(perYearRadios[0]);

    expect(
      screen.getByLabelText('Taxa de Selic Over anual em 2026'),
    ).toBeInTheDocument();
    expect(screen.getAllByLabelText('Taxa anual (%)')).toHaveLength(3);
  });
});
