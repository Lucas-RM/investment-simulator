using InvestmentSimulator.Domain.Enums;
using InvestmentSimulator.Domain.Exceptions;

namespace InvestmentSimulator.Domain.Entities;

/// <summary>
/// A saved simulation in the history catalog (ERS section 26).
/// Holds metadata (name, date, type, observations) plus the full
/// <see cref="Simulation"/> aggregate so it can be loaded and re-run.
/// </summary>
public sealed class SimulationHistoryEntry
{
    public SimulationHistoryEntry(
        string name,
        DateOnly date,
        string observations,
        Simulation simulation,
        Guid? id = null)
    {
        ArgumentNullException.ThrowIfNull(simulation);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainValidationException("Simulation history name is required.");
        }

        if (date == default)
        {
            throw new DomainValidationException("Simulation history date is required and must be a valid date.");
        }

        ArgumentNullException.ThrowIfNull(observations);

        Id = id ?? Guid.NewGuid();
        Name = name.Trim();
        Date = date;
        Type = simulation.Type;
        Observations = observations;
        Simulation = simulation;
    }

    /// <summary>Unique identifier of the history entry.</summary>
    public Guid Id { get; }

    /// <summary>User-defined name of the saved simulation.</summary>
    public string Name { get; }

    /// <summary>Date associated with the history entry (typically when it was saved).</summary>
    public DateOnly Date { get; }

    /// <summary>Investment type of the saved simulation (CDB or Tesouro Selic).</summary>
    public InvestmentType Type { get; }

    /// <summary>Optional free-text notes about the simulation.</summary>
    public string Observations { get; }

    /// <summary>Full simulation inputs required to reload and re-run the calculation.</summary>
    public Simulation Simulation { get; }
}
