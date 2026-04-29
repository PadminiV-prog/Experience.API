using ExperienceApi.Model;

namespace ExperienceApi.Contracts;

/// <summary>
/// Sends HTTP requests to downstream APIs.
/// </summary>
public interface IHttpClientService
{
    /// <summary>
    /// Sends a POST request to a downstream system API.
    /// </summary>
    /// <param name="model">Request model containing URL and payload details.</param>
    /// <param name="token">Bearer token for authorization.</param>
    /// <param name="correlationId">Correlation identifier header value.</param>
    /// <returns>The raw response content.</returns>
    Task<string> PostAsync(ClientRequestModel model, string token, string correlationId);
}
