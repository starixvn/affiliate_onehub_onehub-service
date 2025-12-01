using System.Text.Json;
using ONEHub.Shared.Services.Interfaces;
using StackExchange.Redis;

namespace ONEHub.Shared.Services;
/// <summary>
/// Redis implementation of cache service
/// </summary>
public class RedisCacheService(IConnectionMultiplexer redis) : ICacheService
{
    private readonly IDatabase db = redis.GetDatabase();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = await db.StringGetAsync(key);
        return value.HasValue ? JsonSerializer.Deserialize<T>(value!) : default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var serialized = JsonSerializer.Serialize(value, JsonOptions);
        await db.StringSetAsync(key, serialized, expiration);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await db.KeyDeleteAsync(key);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return await db.KeyExistsAsync(key);
    }
}
