using System.Net.Http.Headers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ONEHub.CronJob.Services.Interfaces;
using ONEHub.Shared.Models;

namespace ONEHub.CronJob.Handlers;

/// <summary>
/// HTTP message handler that automatically adds bearer token authentication to outgoing requests
/// </summary>
public class AuthHeaderHandler : DelegatingHandler
{
    private const string TOKEN_MEMORY_CACHE = "TOKEN_MEMORY_12B_V2_CRONJOBS_CACHE";
    private readonly ITokenApi _tokenApi;
    private readonly IMemoryCache _memoryCache;
    private readonly TokenRequest _token;

    public AuthHeaderHandler(ITokenApi tokenApi, IMemoryCache memoryCache, IOptions<TokenRequest> token)
    {
        _tokenApi = tokenApi ?? throw new ArgumentNullException(nameof(tokenApi));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _token = token.Value ?? throw new ArgumentNullException(nameof(token));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _memoryCache.GetOrCreateAsync(TOKEN_MEMORY_CACHE, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_token.CacheTimeOut);
            var tokenInfo = await _tokenApi.GetTokenAsync(_token, cancellationToken);
            return tokenInfo.AccessToken;
        });

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}

