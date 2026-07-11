import {
  clearSimulationDraft,
  draftStorageKey,
  loadSimulationDraft,
  saveSimulationDraft,
} from '@/utils/simulationDraftStorage';
import { createEmptyCdbDraft } from '@/types/simulationDraft';
import { InvestmentType } from '@/types/investment';

describe('simulationDraftStorage', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it('returns an empty draft when nothing is stored', () => {
    expect(loadSimulationDraft(InvestmentType.Cdb)).toEqual(
      createEmptyCdbDraft(),
    );
  });

  it('persists and reloads a draft', () => {
    const draft = createEmptyCdbDraft();
    draft.generalInputs = {
      initialAmount: '1000',
      startDate: '2026-01-01',
      endDate: '2027-01-01',
    };
    draft.contributions = [{ date: '2026-03-01', amount: '500' }];
    draft.contributionsConfirmed = true;

    saveSimulationDraft(draft);

    expect(loadSimulationDraft(InvestmentType.Cdb)).toEqual(draft);
    expect(localStorage.getItem(draftStorageKey(InvestmentType.Cdb))).toContain(
      '1000',
    );
  });

  it('clears a stored draft', () => {
    saveSimulationDraft(createEmptyCdbDraft());
    clearSimulationDraft(InvestmentType.Cdb);

    expect(loadSimulationDraft(InvestmentType.Cdb)).toEqual(
      createEmptyCdbDraft(),
    );
  });

  it('ignores corrupted storage payloads', () => {
    localStorage.setItem(
      draftStorageKey(InvestmentType.Cdb),
      '{not-valid-json',
    );

    expect(loadSimulationDraft(InvestmentType.Cdb)).toEqual(
      createEmptyCdbDraft(),
    );
  });
});
