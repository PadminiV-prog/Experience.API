using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using ExperienceApi;
using ExperienceApi.Contracts;
using ExperienceApi.Middleware;
using ExperienceApi.ServiceImplementation;
using Microsoft.Azure.Functions.Worker;
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

        services.AddHttpClient("ExperienceApiHttpClient");
        services.AddMemoryCache();

        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IHttpClientService, HttpClientService>();
        services.AddScoped<IExperienceService, ExperienceService>();

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

await host.RunAsync();
