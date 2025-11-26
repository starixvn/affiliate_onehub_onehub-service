namespace ONEHub.Api.Endpoints;

/// <summary>
/// Data endpoints using Minimal API
/// </summary>
public static class DataEndpoints
{
    public static void MapEndpoints(WebApplication app)
    {
        _ = app.MapGroup("/api/data")
            .WithTags("Data");
    }
}
