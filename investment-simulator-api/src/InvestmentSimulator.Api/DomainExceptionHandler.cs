using InvestmentSimulator.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

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
    public static IActionResult Execute(Func<IActionResult> action)
    {
        try
        {
            return action();
        }
        catch (DomainValidationException ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }
}
