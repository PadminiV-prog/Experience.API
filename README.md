# GSI iHUB Experience API

Azure Functions v4 isolated-worker API (.NET 10) that processes experience requests and routes them to downstream system APIs through the GSI iHUB platform.

---

## Solution Structure

```
GSI.IHUB.Experience.Service.sln
├── GSI.IHUB.Experience.Service/          # Azure Functions project (main API)
│   ├── Configuration/                    # Per-environment JSON config files
│   ├── Contracts/                        # Service interfaces
│   ├── Functions/                        # HTTP-triggered function entry points
│   ├── Helpers/                          # Service implementations & shared utilities
│   ├── Middleware/                       # Functions worker middleware pipeline
│   ├── Model/                            # Request/response models and constants
│   ├── ServiceImplementation/            # Core business logic
│   ├── AppSettings.cs                    # Strongly-typed configuration model
│   ├── Program.cs                        # Host builder and DI registration
│   ├── host.json                         # Azure Functions runtime configuration
│   └── local.settings.json              # Local development settings (not published)
└── GSI.IHUB.Experience.Service.Tests/   # xUnit unit test project
    ├── Functions/
    ├── Helpers/
    ├── Middleware/
    ├── ServiceImplementation/
    └── Setup/
```

---

## Projects

### `GSI.IHUB.Experience.Service`

The main Azure Functions project. Exposes a single HTTP-triggered endpoint:

| Method | Route                   | Auth              | Description                            |
|--------|-------------------------|-------------------|----------------------------------------|
| POST   | `/api/experience/process` | Function key (Bearer JWT) | Validates the request, acquires an OAuth2 token, and forwards the payload to the configured downstream system API. |

#### Middleware Pipeline

Middleware executes in registration order (outermost first):

1. **`ExceptionHandlingMiddleware`** — catches any unhandled exception and returns a standardised RFC 7807 `application/problem+json` 500 response, including the correlation ID.
2. **`RequestHeaderMiddleware`** — reads the `x-correlation-id` request header (or generates a new GUID if absent) and stores it in `FunctionContext.Items` for use throughout the pipeline.

#### Services

| Interface | Implementation | Description |
|-----------|---------------|-------------|
| `IValidationService` | `ValidationService` | Validates that the request body is valid JSON containing non-empty `Route` and `Payload` fields. |
| `ITokenService` | `TokenService` | Acquires an OAuth2 client-credentials token via MSAL and caches it in-memory until 5 minutes before expiry. |
| `IHttpClientService` | `HttpClientService` | Posts the request to the downstream API using a named `HttpClient`, attaching the Bearer token, correlation ID, subscription key, and API version headers. |
| `IExperienceService` | `ExperienceService` | Orchestrates validation, token acquisition, and the downstream HTTP call; reads CSV data from Azure Blob Storage when required. |

#### Shared Utilities

- **`ProblemDetailsResponseHelper`** — shared static factory that builds RFC 7807 `ExperienceProblemDetails` responses. Provides `GetProblemType(HttpStatusCode)` (maps status codes to RFC 9110 URI references) and `CreateProblemResponseAsync(...)` (serialises the problem details and writes the `application/problem+json` response). Used by both `ProcessRequestFunction` and `ExceptionHandlingMiddleware`.

#### Error Responses

All error responses conform to [RFC 7807](https://datatracker.ietf.org/doc/html/rfc7807) (`application/problem+json`):

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Invalid request payload.",
  "instance": "/api/experience/process",
  "correlationId": "a1b2c3d4-..."
}
```

Status codes and their RFC 9110 type URIs:

| Status | Type URI |
|--------|----------|
| 400 Bad Request | `rfc9110#section-15.5.1` |
| 401 Unauthorized | `rfc9110#section-15.5.2` |
| 403 Forbidden | `rfc9110#section-15.5.4` |
| 500 Internal Server Error | `rfc9110#section-15.6.1` |

#### OpenAPI / Swagger

An interactive Swagger UI is available at the Functions host root when running locally. The spec is served at OpenAPI v3 with the title **"GSI iHUB Experience API"**.

---

## Configuration

### Environment Selection

The host selects a per-environment JSON file from the `Configuration/` folder based on the `AZURE_ENVIRONMENT` environment variable (defaults to `dev`):

| Value | File |
|-------|------|
| `dev` | `Configuration/dev.json` |
| `qa` | `Configuration/qa.json` |
| `uat` | `Configuration/uat.json` |
| `perf` | `Configuration/perf.json` |
| `prd` | `Configuration/prd.json` |
| `dr` | `Configuration/dr.json` |
| `dvhf` | `Configuration/dvhf.json` |
| `uthf` | `Configuration/uthf.json` |

### AppSettings

All settings live under the `AppSettings` section:

| Key | Description |
|-----|-------------|
| `KeyVaultUri` | Azure Key Vault URI. When set, secrets are loaded from Key Vault at startup using Managed Identity. |
| `ManagedIdentityClientId` | User-assigned Managed Identity client ID. Leave empty to use system-assigned identity. |
| `SystemApiBaseUrl` | Base URL of the downstream system API. |
| `SystemApiSubscriptionKey` | Ocp-Apim-Subscription-Key header value for the downstream API. |
| `SystemApiVersion` | API version appended as `?api-version=` query parameter. |
| `B2BTenantId` | Azure AD B2B tenant ID. |
| `B2BTenantAuthorityUrl` | MSAL authority URL (`https://login.microsoftonline.com/{tenant-id}`). |
| `B2BClientId` | Client ID for OAuth2 client-credentials flow. |
| `B2BClientSecret` | Client secret for OAuth2 client-credentials flow. |
| `BlobEndpoint` | Azure Blob Storage account endpoint URL. |
| `BlobContainerName` | Blob container name for CSV file reads. |
| `CacheKeySystemApi` | In-memory cache key for the downstream API token (defaults to `SystemApiToken`). |

### Local Development

Create or update `local.settings.json` with your local values (this file is never published):

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "AZURE_ENVIRONMENT": "dev"
  }
}
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Azure Functions Core Tools v4](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local)

### Run Locally

```bash
cd GSI.IHUB.Experience.Service
func start
```

### Build

```bash
dotnet build
```

### Test

```bash
dotnet test
```

---

## Tests — `GSI.IHUB.Experience.Service.Tests`

xUnit test project (Moq for mocking). Covers:

| Area | Test file |
|------|-----------|
| `ProcessRequestFunction` | `Functions/ProcessRequestFunctionTests.cs` |
| `ExceptionHandlingMiddleware` | `Middleware/ExceptionHandlingMiddlewareTests.cs` |
| `ProblemDetailsResponseHelper` | `Helpers/ProblemDetailsResponseHelperTests.cs` |
| `HttpClientService` | `Helpers/HttpClientServiceTests.cs` |
| `TokenService` | `Helpers/TokenServiceTests.cs` |
| `ValidationService` | `Helpers/ValidationServiceTests.cs` |

```bash
dotnet test --logger "console;verbosity=normal"
```

---

## Key Dependencies

| Package | Purpose |
|---------|---------|
| `Microsoft.Azure.Functions.Worker` v2.51 | Azure Functions isolated worker host |
| `Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore` v2.1 | ASP.NET Core HTTP integration |
| `Microsoft.Azure.Functions.Worker.Extensions.OpenApi` v1.6 | OpenAPI / Swagger support |
| `Microsoft.Identity.Client` v4.83 | MSAL — OAuth2 token acquisition |
| `Azure.Identity` v1.21 | Managed Identity / DefaultAzureCredential |
| `Azure.Security.KeyVault.Secrets` v4.10 | Azure Key Vault secrets |
| `Azure.Storage.Blobs` v12.27 | Azure Blob Storage access |
| `Microsoft.ApplicationInsights.WorkerService` v2.22 | Application Insights telemetry |
| `Newtonsoft.Json` v13.0.4 | JSON serialisation |
