import { Link } from 'react-router-dom'
import { paths } from '@/routes/paths'

export function NotFoundPage() {
  return (
    <section>
      <h1>Página não encontrada</h1>
      <p>A rota solicitada não existe.</p>
      <p>
        <Link to={paths.home}>Voltar ao início</Link>
      </p>
    </section>
  )
}
