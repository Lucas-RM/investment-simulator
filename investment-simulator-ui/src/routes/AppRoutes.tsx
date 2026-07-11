import { Navigate, Route, Routes } from 'react-router-dom';
import { AppLayout } from '@/layouts/AppLayout';
import { HomePage } from '@/pages/HomePage';
import { GeneralInputsStepPage } from '@/pages/GeneralInputsStepPage';
import { ContributionsStepPage } from '@/pages/ContributionsStepPage';
import { RatesStepPage } from '@/pages/RatesStepPage';
import { ComparisonPage } from '@/pages/ComparisonPage';
import { HistoryPage } from '@/pages/HistoryPage';
import { NotFoundPage } from '@/pages/NotFoundPage';
import { paths } from '@/routes/paths';
import { InvestmentType } from '@/types/investment';

export function AppRoutes() {
  return (
    <Routes>
      <Route element={<AppLayout />}>
        <Route index element={<HomePage />} />

        <Route
          path={paths.cdb}
          element={
            <GeneralInputsStepPage investmentType={InvestmentType.Cdb} />
          }
        />
        <Route
          path={paths.cdbContributions}
          element={
            <ContributionsStepPage investmentType={InvestmentType.Cdb} />
          }
        />
        <Route
          path={paths.cdbRates}
          element={<RatesStepPage investmentType={InvestmentType.Cdb} />}
        />

        <Route
          path={paths.tesouro}
          element={
            <GeneralInputsStepPage
              investmentType={InvestmentType.TesouroSelic}
            />
          }
        />
        <Route
          path={paths.tesouroContributions}
          element={
            <ContributionsStepPage
              investmentType={InvestmentType.TesouroSelic}
            />
          }
        />
        <Route
          path={paths.tesouroRates}
          element={
            <RatesStepPage investmentType={InvestmentType.TesouroSelic} />
          }
        />

        <Route path={paths.compare} element={<ComparisonPage />} />
        <Route path={paths.history} element={<HistoryPage />} />
        <Route path="/simulate" element={<Navigate to={paths.cdb} replace />} />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  );
}
