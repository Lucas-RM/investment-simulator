namespace InvestmentSimulator.Api.Contracts;

/// <summary>Detalhamento por aporte ao exportar um resultado existente.</summary>
public sealed class ContributionDetailRequest
{
    /// <summary>Data do aporte.</summary>
    public DateOnly Date { get; init; }

    /// <summary>Valor aportado em R$.</summary>
    public decimal Amount { get; init; }

    /// <summary>Saldo bruto do aporte em R$.</summary>
    public decimal GrossBalance { get; init; }

    /// <summary>Rendimento bruto do aporte em R$.</summary>
    public decimal GrossYield { get; init; }

    /// <summary>Dias corridos investidos.</summary>
    public int CalendarDaysInvested { get; init; }

    /// <summary>Dias úteis com rendimento aplicado.</summary>
    public int BusinessDaysInvested { get; init; }

    /// <summary>IR do aporte em R$.</summary>
    public decimal IncomeTax { get; init; }

    /// <summary>IOF do aporte em R$.</summary>
    public decimal Iof { get; init; }
}
