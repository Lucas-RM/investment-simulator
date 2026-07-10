import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { AppRoutes } from '@/routes/AppRoutes'
import { paths } from '@/routes/paths'

function renderAt(path: string) {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <AppRoutes />
    </MemoryRouter>,
  )
}

describe('App routing', () => {
  it('renders the home page on /', () => {
    renderAt(paths.home)

    expect(
      screen.getByRole('heading', { name: 'Simulador de Investimentos' }),
    ).toBeInTheDocument()
  })

  it('navigates to the CDB stub page', async () => {
    const user = userEvent.setup()
    renderAt(paths.home)

    await user.click(screen.getByRole('link', { name: 'Simular CDB' }))

    expect(
      screen.getByRole('heading', { name: 'Simulação CDB' }),
    ).toBeInTheDocument()
  })

  it('renders the Tesouro Selic stub page', () => {
    renderAt(paths.tesouro)

    expect(
      screen.getByRole('heading', { name: 'Simulação Tesouro Selic' }),
    ).toBeInTheDocument()
  })

  it('renders the not found page for unknown routes', () => {
    renderAt('/rota-inexistente')

    expect(
      screen.getByRole('heading', { name: 'Página não encontrada' }),
    ).toBeInTheDocument()
  })
})
