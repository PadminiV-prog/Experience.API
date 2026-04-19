using Azure.Identity;
using Azure.Storage.Blobs;
using ExperienceApi.Contracts;
using ExperienceApi.Model;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace ExperienceApi.ServiceImplementation;

public class ExperienceService : IExperienceService
{
    private readonly AppSettings _appSettings;
    private readonly ITokenService _tokenService;
    private readonly IHttpClientService _httpClientService;

    public ExperienceService(
        IOptions<AppSettings> appSettings,
        ITokenService tokenService,
        IHttpClientService httpClientService)
    {
        _appSettings = appSettings.Value;
        _tokenService = tokenService;
        _httpClientService = httpClientService;
    }

    public async Task<string> ProcessRequestAsync(ExperienceRequest request, string correlationId, string sourceId)
    {
        var requestData = JsonConvert.SerializeObject(request);

        var clientRequestModel = new ClientRequestModel
        {
            BaseUrl = _appSettings.SystemApiBaseUrl,
            Url = request.Route,
            ClientId = _appSettings.B2BClientId,
            Secret = _appSettings.B2BClientSecret,
            AuthorityUrl = _appSettings.B2BTenantAuthorityUrl,
            SubscriptionKey = _appSettings.SystemApiSubscriptionKey,
            ApiVersion = _appSettings.SystemApiVersion,
            CacheKey = string.IsNullOrWhiteSpace(_appSettings.CacheKeySystemApi)
                ? ApplicationConstants.CacheKeySystemApi
                : _appSettings.CacheKeySystemApi,
            Scope = $"api://{_appSettings.B2BClientId}/.default",
            Data = requestData,
            CorrelationId = correlationId,
            SourceId = sourceId
        };

        var token = await _tokenService.GetTokenAsync(clientRequestModel);
        var response = await _httpClientService.PostAsync(clientRequestModel, token, correlationId, sourceId);

        return string.IsNullOrWhiteSpace(response) ? "NoResponse" : response;
    }

    public async Task<string> ReadCsvFromBlobAsync(string containerName, string blobName)
    {
        var managedIdentityId = string.IsNullOrWhiteSpace(_appSettings.ManagedIdentityClientId)
            ? ManagedIdentityId.SystemAssigned
            : ManagedIdentityId.FromUserAssignedClientId(_appSettings.ManagedIdentityClientId);

        var managedIdentityCredential = new ManagedIdentityCredential(managedIdentityId);

        var blobServiceClient = new BlobServiceClient(new Uri(_appSettings.BlobEndpoint), managedIdentityCredential);
        var blobClient = blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(blobName);
        var downloadResult = await blobClient.DownloadStreamingAsync();
        using var reader = new StreamReader(downloadResult.Value.Content);
        return await reader.ReadToEndAsync();
    }
}
