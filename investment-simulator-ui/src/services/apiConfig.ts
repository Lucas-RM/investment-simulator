/**
 * Base URL for the Investment Simulator API (`VITE_API_BASE_URL` from `.env`).
 *
 * - Development: same-origin (`''`) so Vite proxies `/simular` to the API
 *   (proxy target comes from `VITE_API_BASE_URL` in `vite.config.ts`).
 * - Production: uses `VITE_API_BASE_URL` directly.
 */
export function getApiBaseUrl(): string {
  if (import.meta.env.DEV) {
    return '';
  }

  const configured = import.meta.env.VITE_API_BASE_URL;
  if (typeof configured === 'string' && configured.trim() !== '') {
    return configured.replace(/\/$/, '');
  }

  return '';
}

export function apiUrl(path: string): string {
  const normalized = path.startsWith('/') ? path : `/${path}`;
  return `${getApiBaseUrl()}${normalized}`;
}
