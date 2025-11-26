using Microsoft.Extensions.Logging;
using ONEHub.Shared.Services.Interfaces;
using ONEHub.Shared.Utilities;

namespace ONEHub.Shared.Services;

public class LoggerService(ILogger<LoggerService> logger) : ILoggerService
{
    public void LogError(string errMsg)
        => logger.LogError(errMsg);

    public void LogError(string errMsg, Exception exception)
        => logger.LogError(errMsg, exception);

    public void LogError(Exception exception)
        => logger.LogError(LoggerUtils.GenerateMessage(exception));

    public void LogInfo(string infoMsg)
        => logger.LogInformation(infoMsg);
}