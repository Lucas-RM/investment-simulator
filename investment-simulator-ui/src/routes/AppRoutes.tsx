import { Navigate, Route, Routes } from 'react-router-dom'
import { AppLayout } from '@/layouts/AppLayout'
import { HomePage } from '@/pages/HomePage'
import { CdbSimulatorPage } from '@/pages/CdbSimulatorPage'
import { TesouroSimulatorPage } from '@/pages/TesouroSimulatorPage'
import { ComparisonPage } from '@/pages/ComparisonPage'
import { HistoryPage } from '@/pages/HistoryPage'
import { NotFoundPage } from '@/pages/NotFoundPage'
import { paths } from '@/routes/paths'

export function AppRoutes() {
  return (
    <Routes>
      <Route element={<AppLayout />}>
        <Route index element={<HomePage />} />
        <Route path={paths.cdb} element={<CdbSimulatorPage />} />
        <Route path={paths.tesouro} element={<TesouroSimulatorPage />} />
        <Route path={paths.compare} element={<ComparisonPage />} />
        <Route path={paths.history} element={<HistoryPage />} />
        <Route path="/simulate" element={<Navigate to={paths.cdb} replace />} />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  )
}
