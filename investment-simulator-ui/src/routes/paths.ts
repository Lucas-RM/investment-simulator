export const paths = {
  home: '/',
  cdb: '/simulate/cdb',
  tesouro: '/simulate/tesouro',
  compare: '/compare',
  history: '/history',
} as const;

export type AppPath = (typeof paths)[keyof typeof paths];
