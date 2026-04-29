using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Experience.API.Helpers
{
    public class HttpClientService : IHttpClientService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpClientService> _logger;

        public HttpClientService(HttpClient httpClient, ILogger<HttpClientService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> GetAsync(string url, string accessToken, string correlationId)
        {
            SetAuthorizationHeader(accessToken, correlationId);
            _logger.LogInformation("GET {Url} | CorrelationId: {CorrelationId}", url, correlationId);
            var response = await _httpClient.GetAsync(url);
            return await ReadResponseAsync(response, correlationId);
        }

        public async Task<string> PostAsync(string url, string requestBody, string accessToken, string correlationId)
        {
            SetAuthorizationHeader(accessToken, correlationId);
            _logger.LogInformation("POST {Url} | CorrelationId: {CorrelationId}", url, correlationId);
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            return await ReadResponseAsync(response, correlationId);
        }

        public async Task<string> PutAsync(string url, string requestBody, string accessToken, string correlationId)
        {
            SetAuthorizationHeader(accessToken, correlationId);
            _logger.LogInformation("PUT {Url} | CorrelationId: {CorrelationId}", url, correlationId);
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(url, content);
            return await ReadResponseAsync(response, correlationId);
        }

        public async Task<string> DeleteAsync(string url, string accessToken, string correlationId)
        {
            SetAuthorizationHeader(accessToken, correlationId);
            _logger.LogInformation("DELETE {Url} | CorrelationId: {CorrelationId}", url, correlationId);
            var response = await _httpClient.DeleteAsync(url);
            return await ReadResponseAsync(response, correlationId);
        }

        private void SetAuthorizationHeader(string accessToken, string correlationId)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);
            _httpClient.DefaultRequestHeaders.Remove("x-correlation-id");
            _httpClient.DefaultRequestHeaders.Add("x-correlation-id", correlationId);
        }

        private async Task<string> ReadResponseAsync(HttpResponseMessage response, string correlationId)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "HTTP request failed. StatusCode: {StatusCode} | CorrelationId: {CorrelationId} | Response: {Content}",
                    response.StatusCode, correlationId, content);
                response.EnsureSuccessStatusCode();
            }

            return content;
        }
    }
}
