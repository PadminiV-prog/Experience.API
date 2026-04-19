namespace ExperienceApi.Model;

public class ClientRequestModel
{
    public string Url { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string Secret { get; set; } = string.Empty;

    public string AuthorityUrl { get; set; } = string.Empty;

    public string SubscriptionKey { get; set; } = string.Empty;

    public string ApiVersion { get; set; } = string.Empty;

    public string CacheKey { get; set; } = string.Empty;

    public string Scope { get; set; } = string.Empty;

    public string Data { get; set; } = string.Empty;

    public string CorrelationId { get; set; } = string.Empty;

    public string SourceId { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = string.Empty;
}
