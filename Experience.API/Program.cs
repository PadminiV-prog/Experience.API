using Experience.API.Configuration;
using Experience.API.Contract;
using Experience.API.Helpers;
using Experience.API.Middleware;
using Experience.API.ServiceImplementation;
using Experience.API.TransformAdapter;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(worker =>
    {
        worker.UseMiddleware<ExceptionHandlingMiddleware>();
    })
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var appSettings = new AppSettings();
        context.Configuration.Bind(appSettings);
        services.AddSingleton(appSettings);

        services.AddHttpClient<IHttpClientService, HttpClientService>();
        services.AddHttpClient<ITokenService, TokenService>();

        services.AddSingleton<IValidationService, ValidationService>();
        services.AddSingleton<ITransformAdapter, ExperienceTransformAdapter>();
        services.AddScoped<IExperienceService, ExperienceService>();

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

await host.RunAsync();
