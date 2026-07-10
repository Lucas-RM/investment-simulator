using System.Collections.Concurrent;
using InvestmentSimulator.Application.History;
using InvestmentSimulator.Domain.Entities;

namespace InvestmentSimulator.Infrastructure.Persistence;

/// <summary>
/// In-memory store for simulation history (ERS section 26).
/// Suitable for tests and local development until a durable store is wired.
/// </summary>
public sealed class InMemorySimulationHistoryRepository : ISimulationHistoryRepository
{
    private readonly ConcurrentDictionary<Guid, SimulationHistoryEntry> _entries = new();

    /// <inheritdoc />
    public SimulationHistoryEntry Save(SimulationHistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        _entries[entry.Id] = entry;
        return entry;
    }

    /// <inheritdoc />
    public SimulationHistoryEntry? GetById(Guid id)
    {
        return _entries.TryGetValue(id, out var entry) ? entry : null;
    }

    /// <inheritdoc />
    public IReadOnlyList<SimulationHistoryEntry> List()
    {
        return _entries.Values
            .OrderByDescending(e => e.Date)
            .ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
