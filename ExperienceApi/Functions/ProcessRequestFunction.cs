using ExperienceApi.Contracts;
using ExperienceApi.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace ExperienceApi.Functions;

public class ProcessRequestFunction
{
    private readonly ILogger<ProcessRequestFunction> _logger;
    private readonly IExperienceService _experienceService;
    private readonly IValidationService _validationService;

    public ProcessRequestFunction(
        ILogger<ProcessRequestFunction> logger,
        IExperienceService experienceService,
        IValidationService validationService)
    {
        _logger = logger;
        _experienceService = experienceService;
        _validationService = validationService;
    }

    [Function("ProcessRequest")]
    [OpenApiOperation(operationId: "ProcessRequest", tags: new[] { "Experience" }, Summary = "Process experience request")]
    [OpenApiSecurity("BearerAuth", SecuritySchemeType.Http, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(ExperienceRequest), Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "Successful response")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(string), Summary = "Bad request")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(string), Summary = "Unauthorized")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(string), Summary = "Forbidden")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(string), Summary = "Server error")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "experience/process")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var correlationId = executionContext.Items.TryGetValue(ApplicationConstants.CorrelationIdHeaderKey, out var correlation)
            ? correlation?.ToString() ?? Guid.NewGuid().ToString()
            : Guid.NewGuid().ToString();

        var sourceId = executionContext.Items.TryGetValue(ApplicationConstants.SourceIdHeaderKey, out var source)
            ? source?.ToString() ?? ApplicationConstants.DefaultSourceId
            : ApplicationConstants.DefaultSourceId;

        try
        {
            string requestBody;
            using (var reader = new StreamReader(req.Body, Encoding.UTF8))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                return await CreateResponseAsync(req, HttpStatusCode.BadRequest, "Invalid request payload.");
            }

            var request = JsonConvert.DeserializeObject<ExperienceRequest>(requestBody);
            if (request is null || !await _validationService.ValidateRequestAsync(request))
            {
                return await CreateResponseAsync(req, HttpStatusCode.BadRequest, "Invalid request payload.");
            }

            var serviceResponse = await _experienceService.ProcessRequestAsync(request, correlationId, sourceId);
            if (string.Equals(serviceResponse, "NoResponse", StringComparison.OrdinalIgnoreCase))
            {
                return await CreateResponseAsync(req, HttpStatusCode.InternalServerError, "No response from downstream service.");
            }

            return await CreateResponseAsync(req, HttpStatusCode.OK, serviceResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request.");
            return await CreateResponseAsync(req, HttpStatusCode.InternalServerError, "An internal server error occurred.");
        }
    }

    private static async Task<HttpResponseData> CreateResponseAsync(HttpRequestData request, HttpStatusCode statusCode, string payload)
    {
        var response = request.CreateResponse(statusCode);
        await response.WriteStringAsync(payload);
        return response;
    }
}
