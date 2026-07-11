import { useCallback, useState } from 'react';
import { InvestmentType } from '@/types/investment';
import type { CdbRatesInput, TesouroRatesInput } from '@/types/rates';
import type {
  CdbSimulationDraft,
  SimulationDraft,
  TesouroSimulationDraft,
} from '@/types/simulationDraft';
import {
  isCdbDraft,
  isTesouroDraft,
  loadSimulationDraft,
  saveSimulationDraft,
} from '@/utils/simulationDraftStorage';

/**
 * Loads and persists a simulation draft for the given investment type.
 * Updates are written to localStorage so a page refresh keeps the data.
 */
export function useSimulationDraft(investmentType: InvestmentType) {
  const [draft, setDraft] = useState<SimulationDraft>(() =>
    loadSimulationDraft(investmentType),
  );

  const updateDraft = useCallback(
    (patch: Partial<SimulationDraft>) => {
      setDraft((current) => {
        const next = {
          ...current,
          ...patch,
          investmentType,
          version: 1 as const,
        } as SimulationDraft;
        saveSimulationDraft(next);
        return next;
      });
    },
    [investmentType],
  );

  const updateCdbRates = useCallback((rates: CdbRatesInput) => {
    setDraft((current) => {
      if (!isCdbDraft(current)) {
        return current;
      }
      const next: CdbSimulationDraft = { ...current, rates };
      saveSimulationDraft(next);
      return next;
    });
  }, []);

  const updateTesouroRates = useCallback((rates: TesouroRatesInput) => {
    setDraft((current) => {
      if (!isTesouroDraft(current)) {
        return current;
      }
      const next: TesouroSimulationDraft = { ...current, rates };
      saveSimulationDraft(next);
      return next;
    });
  }, []);

  return { draft, updateDraft, updateCdbRates, updateTesouroRates };
}
