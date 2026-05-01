using Newtonsoft.Json;

namespace GSI.IHUB.Experience.Service.Model;

/// <summary>
/// RFC 7807 / ASP.NET Core ProblemDetails error model returned to callers on error responses.
/// </summary>
public class ExperienceProblemDetails
{
    /// <summary>A URI reference that identifies the problem type.</summary>
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>A short, human-readable summary of the problem type.</summary>
    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>The HTTP status code.</summary>
    [JsonProperty("status")]
    public int Status { get; set; }

    /// <summary>A human-readable explanation specific to this occurrence of the problem.</summary>
    [JsonProperty("detail")]
    public string Detail { get; set; } = string.Empty;

    /// <summary>A URI reference that identifies the specific occurrence of the problem.</summary>
    [JsonProperty("instance", NullValueHandling = NullValueHandling.Ignore)]
    public string? Instance { get; set; }

    /// <summary>The correlation identifier for end-to-end request tracing.</summary>
    [JsonProperty("correlationId")]
    public string CorrelationId { get; set; } = string.Empty;
}
