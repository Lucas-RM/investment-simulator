/**
 * Parses a decimal string for API payloads.
 * Returns null when the value is empty or not a valid decimal literal.
 * Does not perform arithmetic — only validates and converts for JSON numbers.
 */
export function parseDecimalString(value: string): number | null {
  const trimmed = value.trim();
  if (trimmed === '') {
    return null;
  }

  if (!/^-?(?:0|[1-9]\d*)(?:\.\d+)?$/.test(trimmed)) {
    return null;
  }

  return Number(trimmed);
}

/** True when the decimal string represents a positive amount (> 0). */
export function isPositiveDecimalString(value: string): boolean {
  const parsed = parseDecimalString(value);
  return parsed !== null && parsed > 0;
}

/** True when the decimal string represents zero (0 or 0.00…). */
export function isZeroDecimalString(value: string): boolean {
  const parsed = parseDecimalString(value);
  return parsed === 0;
}
