using ExperienceApi.Contracts;
using ExperienceApi.Model;
using System.Net.Http.Headers;
using System.Text;

namespace ExperienceApi.ServiceImplementation;

public class HttpClientService : IHttpClientService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HttpClientService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> PostAsync(ClientRequestModel model, string token, string correlationId, string sourceId)
    {
        var httpClient = _httpClientFactory.CreateClient("ExperienceApiHttpClient");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        httpClient.DefaultRequestHeaders.Remove(ApplicationConstants.CorrelationIdHeaderKey);
        httpClient.DefaultRequestHeaders.Remove(ApplicationConstants.SourceIdHeaderKey);
        httpClient.DefaultRequestHeaders.Remove("Ocp-Apim-Subscription-Key");
        httpClient.DefaultRequestHeaders.Remove("api-version");

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation(ApplicationConstants.CorrelationIdHeaderKey, correlationId);
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation(ApplicationConstants.SourceIdHeaderKey, sourceId);
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Ocp-Apim-Subscription-Key", model.SubscriptionKey);
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("api-version", model.ApiVersion);

        var requestUri = BuildRequestUri(model.BaseUrl, model.Url, model.ApiVersion);
        var content = new StringContent(model.Data, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(requestUri, content);
        return await response.Content.ReadAsStringAsync();
    }

    private static string BuildRequestUri(string baseUrl, string route, string apiVersion)
    {
        var normalizedBaseUrl = baseUrl.TrimEnd('/');
        var normalizedRoute = route.TrimStart('/');

        if (string.IsNullOrWhiteSpace(apiVersion))
        {
            return $"{normalizedBaseUrl}/{normalizedRoute}";
        }

        return $"{normalizedBaseUrl}/{normalizedRoute}?api-version={Uri.EscapeDataString(apiVersion)}";
    }
}
