import { postJson } from '@/services/httpClient';
import type {
  SimulateCdbRequest,
  SimulateTesouroRequest,
  SimulationResultResponse,
} from '@/types/simulationApi';

/** POST /simular/cdb */
export function simulateCdb(
  request: SimulateCdbRequest,
  signal?: AbortSignal,
): Promise<SimulationResultResponse> {
  return postJson<SimulationResultResponse>('/simular/cdb', request, signal);
}

/** POST /simular/tesouro */
export function simulateTesouro(
  request: SimulateTesouroRequest,
  signal?: AbortSignal,
): Promise<SimulationResultResponse> {
  return postJson<SimulationResultResponse>('/simular/tesouro', request, signal);
}
