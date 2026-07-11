import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { GeneralInputsForm } from '@/components/simulation/GeneralInputsForm'
import { InvestmentType } from '@/types/investment'
import { paths } from '@/routes/paths'

function renderForm(
  defaultInvestmentType: InvestmentType = InvestmentType.Cdb,
) {
  const onValidSubmit = vi.fn()

  render(
    <MemoryRouter initialEntries={[paths.cdb]}>
      <Routes>
        <Route
          path={paths.cdb}
          element={
            <GeneralInputsForm
              defaultInvestmentType={defaultInvestmentType}
              onValidSubmit={onValidSubmit}
            />
          }
        />
        <Route path={paths.tesouro} element={<p>Página Tesouro</p>} />
      </Routes>
    </MemoryRouter>,
  )

  return { onValidSubmit }
}

describe('GeneralInputsForm', () => {
  it('renders general input fields', () => {
    renderForm()

    expect(
      screen.getByRole('group', { name: 'Entradas gerais' }),
    ).toBeInTheDocument()
    expect(screen.getByLabelText('Tipo de investimento')).toBeInTheDocument()
    expect(screen.getByLabelText('Valor inicial (R$)')).toBeInTheDocument()
    expect(screen.getByLabelText('Data inicial')).toBeInTheDocument()
    expect(screen.getByLabelText('Data de resgate')).toBeInTheDocument()
  })

  it('shows validation errors for empty required fields', async () => {
    const user = userEvent.setup()
    const { onValidSubmit } = renderForm()

    await user.clear(screen.getByLabelText('Valor inicial (R$)'))
    await user.click(screen.getByRole('button', { name: 'Continuar' }))

    const alerts = screen.getAllByRole('alert')
    expect(alerts.map((node) => node.textContent)).toEqual(
      expect.arrayContaining([
        expect.stringMatching(/informe o valor inicial/i),
        expect.stringMatching(/informe a data inicial/i),
        expect.stringMatching(/informe a data de resgate/i),
      ]),
    )
    expect(onValidSubmit).not.toHaveBeenCalled()
  })

  it('submits valid general inputs', async () => {
    const user = userEvent.setup()
    const { onValidSubmit } = renderForm()

    const amountInput = screen.getByLabelText('Valor inicial (R$)')
    await user.clear(amountInput)
    await user.type(amountInput, '10000.50')
    await user.type(screen.getByLabelText('Data inicial'), '2026-01-15')
    await user.type(screen.getByLabelText('Data de resgate'), '2027-01-15')
    await user.click(screen.getByRole('button', { name: 'Continuar' }))

    expect(onValidSubmit).toHaveBeenCalledWith({
      investmentType: InvestmentType.Cdb,
      initialAmount: '10000.50',
      startDate: '2026-01-15',
      endDate: '2027-01-15',
    })
    expect(
      screen.getByText(/entradas gerais válidas\. continue com os aportes/i),
    ).toBeInTheDocument()
  })

  it('navigates to Tesouro when investment type changes', async () => {
    const user = userEvent.setup()
    renderForm()

    await user.selectOptions(
      screen.getByLabelText('Tipo de investimento'),
      InvestmentType.TesouroSelic,
    )

    expect(screen.getByText('Página Tesouro')).toBeInTheDocument()
  })
})
