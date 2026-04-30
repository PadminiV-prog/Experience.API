using GSI.IHUB.Experience.Service.Contracts;
using GSI.IHUB.Experience.Service.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Client;

namespace GSI.IHUB.Experience.Service.Helpers;

public class TokenService : ITokenService
{
    private readonly IMemoryCache _memoryCache;

    public TokenService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public async Task<string> GetTokenAsync(ClientRequestModel model)
    {
        if (model is null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        if (_memoryCache.TryGetValue(model.CacheKey, out string? cachedToken) && !string.IsNullOrWhiteSpace(cachedToken))
        {
            return cachedToken;
        }

        IConfidentialClientApplication app = ConfidentialClientApplicationBuilder
            .Create(model.ClientId)
            .WithClientSecret(model.Secret)
            .WithAuthority(model.AuthorityUrl)
            .Build();

        var authResult = await app.AcquireTokenForClient(new[] { model.Scope }).ExecuteAsync();
        var token = authResult.AccessToken;

        var cacheDuration = authResult.ExpiresOn - DateTimeOffset.UtcNow - TimeSpan.FromMinutes(5);
        if (cacheDuration <= TimeSpan.Zero)
        {
            cacheDuration = TimeSpan.FromMinutes(1);
        }

        _memoryCache.Set(model.CacheKey, token, cacheDuration);

        return token;
    }
}
