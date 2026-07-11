import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { PerYearRatesModal } from '@/components/simulation/PerYearRatesModal';

describe('PerYearRatesModal', () => {
  it('applies filled rates and closes', async () => {
    const user = userEvent.setup();
    const onApply = vi.fn();
    const onClose = vi.fn();

    render(
      <PerYearRatesModal
        open
        title="CDI anual"
        rates={[
          { year: 2026, rate: '' },
          { year: 2027, rate: '' },
        ]}
        onApply={onApply}
        onClose={onClose}
      />,
    );

    const dialog = screen.getByRole('dialog');
    await user.type(
      within(dialog).getByLabelText('Taxa de CDI anual em 2026'),
      '15',
    );
    await user.type(
      within(dialog).getByLabelText('Taxa de CDI anual em 2027'),
      '13',
    );
    await user.click(
      within(dialog).getByRole('button', { name: 'Aplicar taxas' }),
    );

    expect(onApply).toHaveBeenCalledWith([
      { year: 2026, rate: '15' },
      { year: 2027, rate: '13' },
    ]);
    expect(onClose).toHaveBeenCalled();
  });

  it('blocks apply when a year rate is empty', async () => {
    const user = userEvent.setup();
    const onApply = vi.fn();

    render(
      <PerYearRatesModal
        open
        title="IPCA anual"
        rates={[
          { year: 2026, rate: '4.5' },
          { year: 2027, rate: '' },
        ]}
        onApply={onApply}
        onClose={vi.fn()}
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Aplicar taxas' }));

    expect(
      screen.getByText(/corrija as taxas destacadas/i),
    ).toBeInTheDocument();
    expect(onApply).not.toHaveBeenCalled();
  });

  it('cancels without applying', async () => {
    const user = userEvent.setup();
    const onApply = vi.fn();
    const onClose = vi.fn();

    render(
      <PerYearRatesModal
        open
        title="Selic Over anual"
        rates={[{ year: 2026, rate: '14' }]}
        onApply={onApply}
        onClose={onClose}
      />,
    );

    await user.click(screen.getByRole('button', { name: 'Cancelar' }));

    expect(onClose).toHaveBeenCalled();
    expect(onApply).not.toHaveBeenCalled();
  });
});
