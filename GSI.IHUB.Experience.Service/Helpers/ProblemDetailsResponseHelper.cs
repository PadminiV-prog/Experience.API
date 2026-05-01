using GSI.IHUB.Experience.Service.Model;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using System.Net;

namespace GSI.IHUB.Experience.Service.Helpers;

/// <summary>
/// Shared factory for building RFC 7807 ProblemDetails HTTP responses.
/// Centralises <see cref="ExperienceProblemDetails"/> construction, JSON serialisation,
/// and the status-code → problem-type URI mapping so that every call site produces a
/// consistent <c>application/problem+json</c> response.
/// </summary>
public static class ProblemDetailsResponseHelper
{
    /// <summary>
    /// Returns the RFC 9110 section URI that corresponds to the given HTTP status code.
    /// </summary>
    public static string GetProblemType(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.BadRequest => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
        HttpStatusCode.Unauthorized => "https://tools.ietf.org/html/rfc9110#section-15.5.2",
        HttpStatusCode.Forbidden => "https://tools.ietf.org/html/rfc9110#section-15.5.4",
        HttpStatusCode.InternalServerError => "https://tools.ietf.org/html/rfc9110#section-15.6.1",
        _ => "about:blank"
    };

    /// <summary>
    /// Creates an <see cref="HttpResponseData"/> with <c>application/problem+json</c> content
    /// populated from a new <see cref="ExperienceProblemDetails"/> instance.
    /// </summary>
    public static async Task<HttpResponseData> CreateProblemResponseAsync(
        HttpRequestData request,
        HttpStatusCode statusCode,
        string title,
        string detail,
        string correlationId)
    {
        var problem = new ExperienceProblemDetails
        {
            Type = GetProblemType(statusCode),
            Title = title,
            Status = (int)statusCode,
            Detail = detail,
            Instance = request.Url.AbsolutePath,
            CorrelationId = correlationId
        };

        var response = request.CreateResponse(statusCode);
        response.Headers.Add("Content-Type", "application/problem+json; charset=utf-8");
        await response.WriteStringAsync(JsonConvert.SerializeObject(problem));
        return response;
    }
}
