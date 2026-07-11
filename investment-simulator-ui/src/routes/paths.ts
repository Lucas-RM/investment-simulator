export const paths = {
  home: '/',
  cdb: '/simulate/cdb',
  cdbContributions: '/simulate/cdb/contributions',
  cdbRates: '/simulate/cdb/rates',
  cdbResult: '/simulate/cdb/result',
  tesouro: '/simulate/tesouro',
  tesouroContributions: '/simulate/tesouro/contributions',
  tesouroRates: '/simulate/tesouro/rates',
  tesouroResult: '/simulate/tesouro/result',
  compare: '/compare',
  history: '/history',
} as const;

export type AppPath = (typeof paths)[keyof typeof paths];
