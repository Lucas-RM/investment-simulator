namespace InvestmentSimulator.Api.Contracts;

/// <summary>Detalhamento por aporte no resultado da simulação.</summary>
public sealed class ContributionDetailResponse
{
    /// <summary>Data do aporte (ajustada para dia útil, se aplicável).</summary>
    public DateOnly Date { get; init; }

    /// <summary>Valor aportado em R$.</summary>
    public decimal Amount { get; init; }

    /// <summary>Saldo bruto do aporte ao final da simulação em R$.</summary>
    public decimal GrossBalance { get; init; }

    /// <summary>Rendimento bruto acumulado do aporte em R$.</summary>
    public decimal GrossYield { get; init; }

    /// <summary>Dias corridos investidos (base de IR e IOF).</summary>
    public int CalendarDaysInvested { get; init; }

    /// <summary>Dias úteis em que o rendimento diário foi aplicado.</summary>
    public int BusinessDaysInvested { get; init; }

    /// <summary>Imposto de Renda (IR) do aporte em R$.</summary>
    public decimal IncomeTax { get; init; }

    /// <summary>IOF do aporte em R$ (zero se dias corridos ≥ 30).</summary>
    public decimal Iof { get; init; }
}
