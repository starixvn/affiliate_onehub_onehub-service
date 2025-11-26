namespace ONEHub.Shared.Services.Interfaces;

public interface ILoggerService
{
    void LogError(string errMsg);

    void LogError(string errMsg, Exception exception);

    void LogError(Exception exception);

    void LogInfo(string infoMsg);
}