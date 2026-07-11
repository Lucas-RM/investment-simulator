import { InvestmentType } from '@/types/investment';
import { paths } from '@/routes/paths';

export type SimulationStepPaths = {
  general: string;
  contributions: string;
  rates: string;
  result: string;
};

export function simulationStepPaths(
  investmentType: InvestmentType,
): SimulationStepPaths {
  if (investmentType === InvestmentType.Cdb) {
    return {
      general: paths.cdb,
      contributions: paths.cdbContributions,
      rates: paths.cdbRates,
      result: paths.cdbResult,
    };
  }

  return {
    general: paths.tesouro,
    contributions: paths.tesouroContributions,
    rates: paths.tesouroRates,
    result: paths.tesouroResult,
  };
}
