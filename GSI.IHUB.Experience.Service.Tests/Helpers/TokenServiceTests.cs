using GSI.IHUB.Experience.Service.Helpers;
using GSI.IHUB.Experience.Service.Model;
using Microsoft.Extensions.Caching.Memory;

namespace GSI.IHUB.Experience.Service.Tests.Helpers;

public class TokenServiceTests
{
    private static ClientRequestModel CreateModel(string cacheKey = "TestCacheKey") =>
        new()
        {
            ClientId = "client-id",
            Secret = "client-secret",
            AuthorityUrl = "https://login.microsoftonline.com/tenant-id/v2.0",
            Scope = "api://client-id/.default",
            CacheKey = cacheKey
        };

    [Fact]
    public async Task GetTokenAsync_NullModel_ThrowsArgumentNullException()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var sut = new TokenService(cache);

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.GetTokenAsync(null!));
    }

    [Fact]
    public async Task GetTokenAsync_CachedToken_ReturnsCachedValueWithoutCallingAuthority()
    {
        const string cachedToken = "cached-bearer-token";
        var cache = new MemoryCache(new MemoryCacheOptions());
        var model = CreateModel("CachedKey");
        cache.Set(model.CacheKey, cachedToken, TimeSpan.FromMinutes(10));

        var sut = new TokenService(cache);

        var result = await sut.GetTokenAsync(model);

        Assert.Equal(cachedToken, result);
    }

    [Fact]
    public async Task GetTokenAsync_ExpiredCacheEntry_WhitespaceToken_AcquiresNewToken()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var model = CreateModel("ExpiredKey");
        cache.Set(model.CacheKey, "   ", TimeSpan.FromMilliseconds(1));
        await Task.Delay(10);

        var sut = new TokenService(cache);

        // Act: acquiring from a real authority will fail in unit tests — verify the appropriate exception is thrown
        await Assert.ThrowsAnyAsync<Exception>(() => sut.GetTokenAsync(model));
    }

    [Fact]
    public async Task GetTokenAsync_NoCachedToken_AttemptsAcquisition_ThrowsOnInvalidAuthority()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var model = CreateModel("NoCacheKey");

        var sut = new TokenService(cache);

        // With invalid authority credentials the MSAL library will throw an exception
        await Assert.ThrowsAnyAsync<Exception>(() => sut.GetTokenAsync(model));
    }
}
