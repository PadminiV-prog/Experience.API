using System.Net.Http.Headers;
using System.Text.Json;
using Experience.API.Configuration;
using Microsoft.Extensions.Logging;

namespace Experience.API.Helpers
{
    public class TokenService : ITokenService
    {
        private readonly HttpClient _httpClient;
        private readonly AppSettings _appSettings;
        private readonly ILogger<TokenService> _logger;

        private string? _cachedToken;
        private DateTime _tokenExpiry = DateTime.MinValue;

        public TokenService(HttpClient httpClient, AppSettings appSettings, ILogger<TokenService> logger)
        {
            _httpClient = httpClient;
            _appSettings = appSettings;
            _logger = logger;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
            {
                return _cachedToken;
            }

            _logger.LogInformation("Requesting new access token from {TokenEndpoint}", _appSettings.TokenEndpoint);

            var tokenRequest = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", _appSettings.ClientId),
                new KeyValuePair<string, string>("client_secret", _appSettings.ClientSecret),
                new KeyValuePair<string, string>("scope", _appSettings.Scope)
            });

            var response = await _httpClient.PostAsync(_appSettings.TokenEndpoint, tokenRequest);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);

            _cachedToken = tokenResponse.GetProperty("access_token").GetString()
                           ?? throw new InvalidOperationException("Access token not found in response.");

            var expiresIn = tokenResponse.TryGetProperty("expires_in", out var expProp)
                ? expProp.GetInt32()
                : 3600;

            _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn - 60);

            return _cachedToken;
        }
    }
}
