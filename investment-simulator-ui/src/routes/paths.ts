export const paths = {
  home: '/',
  cdb: '/simulate/cdb',
  cdbContributions: '/simulate/cdb/contributions',
  cdbRates: '/simulate/cdb/rates',
  tesouro: '/simulate/tesouro',
  tesouroContributions: '/simulate/tesouro/contributions',
  tesouroRates: '/simulate/tesouro/rates',
  compare: '/compare',
  history: '/history',
} as const;

export type AppPath = (typeof paths)[keyof typeof paths];
