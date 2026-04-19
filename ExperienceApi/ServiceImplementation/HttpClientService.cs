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
        var requestUri = BuildRequestUri(model.BaseUrl, model.Url, model.ApiVersion);
        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = new StringContent(model.Data, Encoding.UTF8, "application/json")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.TryAddWithoutValidation(ApplicationConstants.CorrelationIdHeaderKey, correlationId);
        request.Headers.TryAddWithoutValidation(ApplicationConstants.SourceIdHeaderKey, sourceId);
        request.Headers.TryAddWithoutValidation("Ocp-Apim-Subscription-Key", model.SubscriptionKey);
        request.Headers.TryAddWithoutValidation("api-version", model.ApiVersion);

        using var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Downstream API request failed. StatusCode={(int)response.StatusCode}, Route={model.Url}, Body={errorBody}");
        }

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
