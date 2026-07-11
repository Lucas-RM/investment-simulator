import { useCallback, useRef, useState } from 'react';
import { ApiClientError } from '@/services/ApiClientError';
import { simulateCdb, simulateTesouro } from '@/services/simulationApi';
import { InvestmentType } from '@/types/investment';
import type { SimulationResultResponse } from '@/types/simulationApi';
import type {
  CdbSimulationDraft,
  SimulationDraft,
  TesouroSimulationDraft,
} from '@/types/simulationDraft';
import {
  mapCdbDraftToRequest,
  mapTesouroDraftToRequest,
} from '@/utils/mapSimulationRequest';

export type SimulationRequestStatus = 'idle' | 'loading' | 'success' | 'error';

export type UseRunSimulationResult = {
  status: SimulationRequestStatus;
  result: SimulationResultResponse | null;
  error: string | null;
  isLoading: boolean;
  run: (draft: SimulationDraft) => Promise<SimulationResultResponse | null>;
  reset: () => void;
};

function toUserMessage(error: unknown): string {
  if (error instanceof ApiClientError) {
    return error.message;
  }

  if (error instanceof Error && error.message) {
    return error.message;
  }

  return 'Ocorreu um erro inesperado ao simular.';
}

/**
 * Runs a CDB or Tesouro simulation against the API, exposing loading / error / result.
 */
export function useRunSimulation(): UseRunSimulationResult {
  const [status, setStatus] = useState<SimulationRequestStatus>('idle');
  const [result, setResult] = useState<SimulationResultResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  const abortRef = useRef<AbortController | null>(null);

  const reset = useCallback(() => {
    abortRef.current?.abort();
    abortRef.current = null;
    setStatus('idle');
    setResult(null);
    setError(null);
  }, []);

  const run = useCallback(async (draft: SimulationDraft) => {
    abortRef.current?.abort();
    const controller = new AbortController();
    abortRef.current = controller;

    setStatus('loading');
    setError(null);
    setResult(null);

    try {
      const response =
        draft.investmentType === InvestmentType.Cdb
          ? await simulateCdb(
              mapCdbDraftToRequest(draft as CdbSimulationDraft),
              controller.signal,
            )
          : await simulateTesouro(
              mapTesouroDraftToRequest(draft as TesouroSimulationDraft),
              controller.signal,
            );

      if (controller.signal.aborted) {
        return null;
      }

      setResult(response);
      setStatus('success');
      return response;
    } catch (cause) {
      if (cause instanceof DOMException && cause.name === 'AbortError') {
        return null;
      }

      const message = toUserMessage(cause);
      setError(message);
      setStatus('error');
      return null;
    } finally {
      if (abortRef.current === controller) {
        abortRef.current = null;
      }
    }
  }, []);

  return {
    status,
    result,
    error,
    isLoading: status === 'loading',
    run,
    reset,
  };
}
