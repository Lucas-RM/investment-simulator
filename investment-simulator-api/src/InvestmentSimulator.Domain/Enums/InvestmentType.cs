namespace InvestmentSimulator.Domain.Enums;

/// <summary>
/// Supported investment types for simulation (ERS sections 1–2).
/// </summary>
public enum InvestmentType
{
    /// <summary>CDB pós-fixado indexado ao CDI.</summary>
    Cdb = 1,

    /// <summary>Tesouro Selic (Selic Over + ágio/deságio).</summary>
    TesouroSelic = 2,
}
