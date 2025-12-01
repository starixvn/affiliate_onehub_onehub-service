using System.Text;
using Microsoft.AspNetCore.Http;

namespace ONEHub.Shared.Utilities;

public static class LoggerUtils
{
    public static string GenerateMessage(Exception exception, HttpContext? context = null)
    {
        var messageBuilder = new StringBuilder();

        messageBuilder.AppendLine($"MESSAGE: {exception.Message}");

        if (context is not null)
        {
            AddRequestInfo(context, messageBuilder);
        }

        const int limitedNumOfInnerException = 3;
        AddExceptionInfo(messageBuilder, exception, limitedNumOfInnerException);

        return messageBuilder.ToString();
    }

    private static void AddRequestInfo(HttpContext? context, StringBuilder builder)
    {
        builder.AppendLine();
        builder.AppendLine("REQUEST INFO");

        var webInfo = GetRequestInfo(context);

        foreach (var item in webInfo)
        {
            builder.AppendLine($"{item.Key}: {item.Value}");
        }
    }

    private static void AddExceptionInfo(StringBuilder builder, Exception exception, int limitedInnerException)
    {
        builder.AppendLine();
        builder.AppendLine("EXCEPTION INFO");
        builder.AppendLine($"Message: {exception.Message}");
        builder.AppendLine($"Source: {exception.Source}");
        builder.AppendLine($"TargetSite: {exception.TargetSite}");
        builder.AppendLine($"StackTrace: {exception.StackTrace}");

        if (exception.InnerException != null && limitedInnerException > 0)
        {
            AddExceptionInfo(builder, exception.InnerException, limitedInnerException - 1);
        }
    }

    private static IDictionary<string, object?> GetRequestInfo(HttpContext? context)
    {
        if (context is null)
        {
            return new Dictionary<string, object?>();
        }

        var request = context.Request;
        return new Dictionary<string, object?>
        {
            [nameof(request.Host)] = request.Host,
            [nameof(request.Path)] = request.Path,
            ["Remote IP"] = context.Connection.RemoteIpAddress
        };
    }
}
