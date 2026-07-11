/**
 * Additional contribution (aporte) from ERS section 4.
 * Monetary values are kept as strings to avoid float precision loss.
 */
export type ContributionInput = {
  /** Contribution date (YYYY-MM-DD). */
  date: string;
  /** Contribution amount in BRL as a decimal string (e.g. "1000.50"). */
  amount: string;
};

/**
 * Row used by the dynamic contributions table (includes a stable client id).
 */
export type ContributionRow = ContributionInput & {
  id: string;
};

export type ContributionRowErrors = {
  date?: string;
  amount?: string;
};

export type ContributionsErrors = Record<string, ContributionRowErrors>;
