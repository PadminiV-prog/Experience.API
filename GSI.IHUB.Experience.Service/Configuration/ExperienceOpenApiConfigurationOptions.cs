using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace GSI.IHUB.Experience.Service.Configuration;

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
