import { render, screen, within } from '@testing-library/react';
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
  it('renders Selic, single ágio decimal, B3 and IPCA fields', () => {
    renderForm();

    expect(
      screen.getByRole('group', { name: 'Taxas — Tesouro Selic' }),
    ).toBeInTheDocument();
    expect(screen.getByText('Selic Over anual')).toBeInTheDocument();
    expect(
      screen.getByLabelText('Ágio / deságio anual (%)'),
    ).toBeInTheDocument();
    expect(screen.getByText('Taxa de custódia B3')).toBeInTheDocument();
    expect(screen.getByText('IPCA anual')).toBeInTheDocument();
    expect(screen.getByLabelText('Ágio / deságio anual (%)')).toHaveValue('0');
  });

  it('shows validation errors when required fields are empty', async () => {
    const user = userEvent.setup();
    const { onValidSubmit } = renderForm();

    await user.clear(screen.getByLabelText('Ágio / deságio anual (%)'));
    await user.click(screen.getByRole('button', { name: 'Simular' }));

    const alerts = screen.getAllByRole('alert');
    expect(alerts.length).toBeGreaterThanOrEqual(4);
    expect(onValidSubmit).not.toHaveBeenCalled();
  });

  it('submits a single annualAgioRate decimal, not a rate list', async () => {
    const user = userEvent.setup();
    const { onValidSubmit } = renderForm();

    const scheduleRateInputs = screen.getAllByLabelText('Taxa anual (%)');
    expect(scheduleRateInputs).toHaveLength(3);

    await user.type(scheduleRateInputs[0], '14.15');
    await user.clear(screen.getByLabelText('Ágio / deságio anual (%)'));
    await user.type(screen.getByLabelText('Ágio / deságio anual (%)'), '0.10');
    await user.type(scheduleRateInputs[1], '0.2');
    await user.type(scheduleRateInputs[2], '4.5');
    await user.click(screen.getByRole('button', { name: 'Simular' }));

    expect(onValidSubmit).toHaveBeenCalledWith({
      selic: { mode: 'single', singleRate: '14.15', rates: [] },
      annualAgioRate: '0.10',
      b3Custody: { mode: 'single', singleRate: '0.2', rates: [] },
      ipca: { mode: 'single', singleRate: '4.5', rates: [] },
    });
  });

  it('accepts negative ágio (premium over par)', async () => {
    const user = userEvent.setup();
    const { onValidSubmit } = renderForm();

    const scheduleRateInputs = screen.getAllByLabelText('Taxa anual (%)');
    await user.type(scheduleRateInputs[0], '14.15');
    await user.clear(screen.getByLabelText('Ágio / deságio anual (%)'));
    await user.type(screen.getByLabelText('Ágio / deságio anual (%)'), '-0.05');
    await user.type(scheduleRateInputs[1], '0');
    await user.type(scheduleRateInputs[2], '4');
    await user.click(screen.getByRole('button', { name: 'Simular' }));

    expect(onValidSubmit).toHaveBeenCalledWith(
      expect.objectContaining({ annualAgioRate: '-0.05' }),
    );
  });

  it('opens per-year modal for Selic without affecting ágio', async () => {
    const user = userEvent.setup();
    renderForm();

    const perYearRadios = screen.getAllByLabelText('Ano a ano');
    expect(perYearRadios).toHaveLength(3);
    await user.click(perYearRadios[0]);

    expect(screen.getAllByLabelText('Taxa anual (%)')).toHaveLength(2);
    expect(
      screen.getByLabelText('Ágio / deságio anual (%)'),
    ).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: /taxas ano a ano/i }));

    const dialog = screen.getByRole('dialog');
    expect(
      within(dialog).getByRole('heading', {
        name: /selic over anual — ano a ano/i,
      }),
    ).toBeInTheDocument();
    expect(
      within(dialog).getByLabelText('Taxa de Selic Over anual em 2026'),
    ).toBeInTheDocument();
  });
});
