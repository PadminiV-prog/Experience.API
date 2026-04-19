namespace ExperienceApi;

public class AppSettings
{
    public string KeyVaultUri { get; set; } = string.Empty;

    public string SystemApiBaseUrl { get; set; } = string.Empty;

    public string SystemApiSubscriptionKey { get; set; } = string.Empty;

    public string SystemApiVersion { get; set; } = string.Empty;

    public string B2BTenantId { get; set; } = string.Empty;

    public string B2BTenantAuthorityUrl { get; set; } = string.Empty;

    public string B2BClientId { get; set; } = string.Empty;

    public string B2BClientSecret { get; set; } = string.Empty;

    public string BlobEndpoint { get; set; } = string.Empty;

    public string BlobContainerName { get; set; } = string.Empty;

    public string ManagedIdentityClientId { get; set; } = string.Empty;

    public string CacheKeySystemApi { get; set; } = string.Empty;
}
