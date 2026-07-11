/**
 * Error thrown when an API call fails (network, HTTP error, or invalid JSON).
 */
export class ApiClientError extends Error {
  readonly status: number | null;
  readonly body: unknown;

  constructor(message: string, status: number | null = null, body: unknown = null) {
    super(message);
    this.name = 'ApiClientError';
    this.status = status;
    this.body = body;
  }
}
