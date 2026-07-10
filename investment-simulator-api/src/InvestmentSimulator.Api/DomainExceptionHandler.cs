using InvestmentSimulator.Domain.Exceptions;

namespace InvestmentSimulator.Api;

/// <summary>
/// Maps domain validation failures to HTTP 400 responses.
/// </summary>
public static class DomainExceptionHandler
{
    /// <summary>
    /// Executes <paramref name="action"/> and returns Bad Request when a
    /// <see cref="DomainValidationException"/> is thrown.
    /// </summary>
    public static IResult Execute(Func<IResult> action)
    {
        try
        {
            return action();
        }
        catch (DomainValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
