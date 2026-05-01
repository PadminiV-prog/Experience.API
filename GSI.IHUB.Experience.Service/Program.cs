using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using GSI.IHUB.Experience.Service;
using GSI.IHUB.Experience.Service.Contracts;
using GSI.IHUB.Experience.Service.Helpers;
using GSI.IHUB.Experience.Service.Middleware;
using GSI.IHUB.Experience.Service.ServiceImplementation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var host = new HostBuilder()
    .ConfigureAppConfiguration((context, config) =>
    {
        var environment = Environment.GetEnvironmentVariable("AZURE_ENVIRONMENT")?.ToLowerInvariant() ?? "dev";

        config.AddJsonFile("host.json", optional: true, reloadOnChange: false)
              .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
              .AddJsonFile($"Configuration/{environment}.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();

        var currentConfig = config.Build();
        var keyVaultUri = currentConfig["AppSettings:KeyVaultUri"] ?? currentConfig["KeyVaultUri"];

        if (!string.IsNullOrWhiteSpace(keyVaultUri))
        {
            var managedIdentityClientId = currentConfig["AppSettings:ManagedIdentityClientId"] ?? currentConfig["ManagedIdentityClientId"];
            var credentialOptions = new DefaultAzureCredentialOptions();

            if (!string.IsNullOrWhiteSpace(managedIdentityClientId))
            {
                credentialOptions.ManagedIdentityClientId = managedIdentityClientId;
            }

            config.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential(credentialOptions));
        }
    })
    .ConfigureFunctionsWebApplication(worker =>
    {
        worker.UseMiddleware<RequestHeaderMiddleware>();
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<AppSettings>(context.Configuration.GetSection("AppSettings"));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<AppSettings>>().Value);

        services.AddHttpClient("ExperienceServiceHttpClient");
        services.AddMemoryCache();

        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IHttpClientService, HttpClientService>();
        services.AddScoped<IExperienceService, ExperienceService>();

        services.AddSingleton<IOpenApiConfigurationOptions, ExperienceOpenApiConfigurationOptions>();

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

await host.RunAsync();

/// <summary>
/// OpenAPI / Swagger document configuration for the GSI iHUB Experience API.
/// Overrides the default metadata (title, description, version) and pins the spec to OpenAPI v3.
/// </summary>
public class ExperienceOpenApiConfigurationOptions : DefaultOpenApiConfigurationOptions
{
    public override OpenApiInfo Info { get; set; } = new OpenApiInfo
    {
        Version = "1.0.0",
        Title = "GSI iHUB Experience API",
        Description = "Processes experience requests and routes them to downstream system APIs through the GSI iHUB platform.",
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    };

    public override OpenApiVersionType OpenApiVersion { get; set; } = OpenApiVersionType.V3;
}
