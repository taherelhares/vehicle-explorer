using Microsoft.AspNetCore.Diagnostics;
using VehicleExplorer.Application.Abstractions;

namespace VehicleExplorer.Api.ErrorHandling;

/// <summary>
/// Turns an unreachable upstream into a 503 with a problem details body, so the client
/// can tell "the provider is down, try again" apart from "you asked for something wrong".
/// </summary>
internal sealed class NhtsaExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<NhtsaExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not NhtsaUnavailableException)
        {
            return false;
        }

        logger.LogError(exception, "Upstream vPIC request failed.");

        httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Status = StatusCodes.Status503ServiceUnavailable,
                Title = "Vehicle data is temporarily unavailable",
                Detail = "The vehicle data service did not respond. Please try again shortly."
            }
        });
    }
}
