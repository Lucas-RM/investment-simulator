namespace InvestmentSimulator.Domain.Exceptions;

/// <summary>
/// Thrown when a domain rule from ERS sections 5 or 27 is violated.
/// </summary>
public sealed class DomainValidationException : Exception
{
    public DomainValidationException(string message)
        : base(message)
    {
    }

    public DomainValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
