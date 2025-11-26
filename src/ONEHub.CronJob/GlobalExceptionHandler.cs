using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using ONEHub.Shared.Services.Interfaces;
using ONEHub.Shared.Utilities;

namespace ONEHub.CronJob;

/// <summary>
/// Global exception handler for the CronJob
/// </summary>
public class GlobalExceptionHandler(ILoggerService logger, IDateTimeService dateTimeService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(LoggerUtils.GenerateMessage(exception));

        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            error = "An error occurred while processing your request.",
            message = exception.Message,
            timestamp = dateTimeService.UtcNow
        };

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}