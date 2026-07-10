using InvestmentSimulator.Domain.Entities;

namespace InvestmentSimulator.Application.History;

/// <summary>
/// Persists and retrieves saved simulations for history (ERS section 26).
/// </summary>
public interface ISimulationHistoryRepository
{
    /// <summary>
    /// Saves a simulation history entry (name, date, type, observations + inputs).
    /// </summary>
    /// <param name="entry">History entry to persist.</param>
    /// <returns>The stored entry (same identity).</returns>
    SimulationHistoryEntry Save(SimulationHistoryEntry entry);

    /// <summary>
    /// Loads a previously saved simulation by identifier.
    /// </summary>
    /// <param name="id">History entry identifier.</param>
    /// <returns>The entry when found; otherwise <see langword="null"/>.</returns>
    SimulationHistoryEntry? GetById(Guid id);

    /// <summary>
    /// Lists all saved simulations ordered by date descending, then name.
    /// </summary>
    IReadOnlyList<SimulationHistoryEntry> List();
}
