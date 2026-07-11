import { apiUrl } from '@/services/apiConfig';
import { ApiClientError } from '@/services/ApiClientError';
import type { ApiErrorBody } from '@/types/simulationApi';

function readErrorMessage(body: unknown, fallback: string): string {
  if (body && typeof body === 'object' && 'error' in body) {
    const message = (body as ApiErrorBody).error;
    if (typeof message === 'string' && message.trim() !== '') {
      return message;
    }
  }

  return fallback;
}

/**
 * JSON POST helper for simulation endpoints.
 * Throws {@link ApiClientError} on network / HTTP failures.
 */
export async function postJson<TResponse>(
  path: string,
  body: unknown,
  signal?: AbortSignal,
): Promise<TResponse> {
  let response: Response;

  try {
    response = await fetch(apiUrl(path), {
      method: 'POST',
      headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(body),
      signal,
    });
  } catch (cause) {
    if (cause instanceof DOMException && cause.name === 'AbortError') {
      throw cause;
    }

    throw new ApiClientError(
      'Não foi possível conectar à API. Verifique se o servidor está em execução.',
      null,
      cause,
    );
  }

  const contentType = response.headers.get('content-type') ?? '';
  const isJson = contentType.includes('application/json');
  const payload: unknown = isJson ? await response.json() : null;

  if (!response.ok) {
    throw new ApiClientError(
      readErrorMessage(
        payload,
        `A API retornou o status ${response.status}.`,
      ),
      response.status,
      payload,
    );
  }

  return payload as TResponse;
}
