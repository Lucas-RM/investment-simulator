import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { ContributionsForm } from '@/components/simulation/ContributionsForm'

const startDate = '2026-01-01'
const endDate = '2027-01-01'

function renderForm(
  onValidSubmit = vi.fn(),
  defaultContributions?: Array<{ date: string; amount: string }>,
) {
  render(
    <ContributionsForm
      startDate={startDate}
      endDate={endDate}
      defaultContributions={defaultContributions}
      onValidSubmit={onValidSubmit}
    />,
  )

  return { onValidSubmit }
}

describe('ContributionsForm', () => {
  it('renders the contributions section and allows adding rows', async () => {
    const user = userEvent.setup()
    renderForm()

    expect(
      screen.getByRole('group', { name: 'Aportes adicionais' }),
    ).toBeInTheDocument()
    expect(
      screen.getByText(/nenhum aporte adicional cadastrado/i),
    ).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: 'Adicionar aporte' }))

    expect(screen.getByLabelText('Data do aporte 1')).toBeInTheDocument()
    expect(screen.getByLabelText('Valor do aporte 1')).toBeInTheDocument()
  })

  it('shows inline validation errors for invalid rows', async () => {
    const user = userEvent.setup()
    const { onValidSubmit } = renderForm()

    await user.click(screen.getByRole('button', { name: 'Adicionar aporte' }))
    await user.click(screen.getByRole('button', { name: 'Validar aportes' }))

    const alerts = screen.getAllByRole('alert')
    expect(alerts.map((node) => node.textContent)).toEqual(
      expect.arrayContaining([
        expect.stringMatching(/informe a data/i),
        expect.stringMatching(/informe o valor/i),
      ]),
    )
    expect(onValidSubmit).not.toHaveBeenCalled()
  })

  it('submits a valid contribution list', async () => {
    const user = userEvent.setup()
    const { onValidSubmit } = renderForm()

    await user.click(screen.getByRole('button', { name: 'Adicionar aporte' }))
    await user.type(screen.getByLabelText('Data do aporte 1'), '2026-03-15')
    await user.type(screen.getByLabelText('Valor do aporte 1'), '1200.50')
    await user.click(screen.getByRole('button', { name: 'Validar aportes' }))

    expect(onValidSubmit).toHaveBeenCalledWith([
      { date: '2026-03-15', amount: '1200.50' },
    ])
    expect(screen.getByText(/aportes válidos/i)).toBeInTheDocument()
  })

  it('allows submitting with zero additional contributions', async () => {
    const user = userEvent.setup()
    const { onValidSubmit } = renderForm()

    await user.click(screen.getByRole('button', { name: 'Validar aportes' }))

    expect(onValidSubmit).toHaveBeenCalledWith([])
  })

  it('adds and removes rows dynamically', async () => {
    const user = userEvent.setup()
    renderForm()

    await user.click(screen.getByRole('button', { name: 'Adicionar aporte' }))
    await user.click(screen.getByRole('button', { name: 'Adicionar aporte' }))

    expect(screen.getByLabelText('Data do aporte 1')).toBeInTheDocument()
    expect(screen.getByLabelText('Data do aporte 2')).toBeInTheDocument()

    await user.click(
      screen.getByRole('button', { name: 'Remover aporte 1' }),
    )

    expect(screen.queryByLabelText('Data do aporte 2')).not.toBeInTheDocument()
    expect(screen.getByLabelText('Data do aporte 1')).toBeInTheDocument()

    const table = screen.getByRole('table')
    expect(within(table).getAllByRole('row')).toHaveLength(2) // header + 1
  })

  it('rejects a contribution after the redemption date', async () => {
    const user = userEvent.setup()
    const { onValidSubmit } = renderForm()

    await user.click(screen.getByRole('button', { name: 'Adicionar aporte' }))
    await user.type(screen.getByLabelText('Data do aporte 1'), '2027-02-01')
    await user.type(screen.getByLabelText('Valor do aporte 1'), '500')
    await user.click(screen.getByRole('button', { name: 'Validar aportes' }))

    expect(screen.getByRole('alert').textContent).toMatch(
      /posterior à data de resgate/i,
    )
    expect(onValidSubmit).not.toHaveBeenCalled()
  })
})
