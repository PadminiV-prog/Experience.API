using GSI.IHUB.Experience.Service.Contracts;
using GSI.IHUB.Experience.Service.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace GSI.IHUB.Experience.Service.Functions;

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
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/problem+json", bodyType: typeof(ExperienceProblemDetails), Summary = "Bad request")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/problem+json", bodyType: typeof(ExperienceProblemDetails), Summary = "Unauthorized")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/problem+json", bodyType: typeof(ExperienceProblemDetails), Summary = "Forbidden")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/problem+json", bodyType: typeof(ExperienceProblemDetails), Summary = "Server error")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "experience/process")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var correlationId = executionContext.Items.TryGetValue(ApplicationConstants.CorrelationIdHeaderKey, out var correlation)
            ? correlation?.ToString() ?? Guid.NewGuid().ToString()
            : Guid.NewGuid().ToString();

        _logger.LogInformation("ProcessRequest function triggered. CorrelationId: {CorrelationId}", correlationId);

        try
        {
            string requestBody;
            using (var reader = new StreamReader(req.Body, Encoding.UTF8))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            if (!_validationService.ValidateRequest(requestBody))
            {
                _logger.LogWarning("Invalid request payload received. CorrelationId: {CorrelationId}", correlationId);
                return await CreateProblemResponseAsync(req, HttpStatusCode.BadRequest, "Bad Request", "Invalid request payload.", correlationId);
            }

            var request = JsonConvert.DeserializeObject<ExperienceRequest>(requestBody)!;

            var serviceResponse = await _experienceService.ProcessRequestAsync(request, correlationId);
            if (string.Equals(serviceResponse, "NoResponse", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("No response received from downstream service. CorrelationId: {CorrelationId}", correlationId);
                return await CreateProblemResponseAsync(req, HttpStatusCode.InternalServerError, "Internal Server Error", "No response from downstream service.", correlationId);
            }

            _logger.LogInformation("ProcessRequest completed successfully. CorrelationId: {CorrelationId}", correlationId);
            return await CreateResponseAsync(req, HttpStatusCode.OK, serviceResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request. CorrelationId: {CorrelationId}", correlationId);
            return await CreateProblemResponseAsync(req, HttpStatusCode.InternalServerError, "Internal Server Error", "An internal server error occurred.", correlationId);
        }
    }

    private static async Task<HttpResponseData> CreateResponseAsync(HttpRequestData request, HttpStatusCode statusCode, string payload)
    {
        var response = request.CreateResponse(statusCode);
        await response.WriteStringAsync(payload);
        return response;
    }

    private static async Task<HttpResponseData> CreateProblemResponseAsync(
        HttpRequestData request,
        HttpStatusCode statusCode,
        string title,
        string detail,
        string correlationId)
    {
        var problem = new ExperienceProblemDetails
        {
            Type = GetProblemType(statusCode),
            Title = title,
            Status = (int)statusCode,
            Detail = detail,
            Instance = request.Url.AbsolutePath,
            CorrelationId = correlationId
        };

        var response = request.CreateResponse(statusCode);
        response.Headers.Add("Content-Type", "application/problem+json; charset=utf-8");
        await response.WriteStringAsync(JsonConvert.SerializeObject(problem));
        return response;
    }

    private static string GetProblemType(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.BadRequest => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
        HttpStatusCode.Unauthorized => "https://tools.ietf.org/html/rfc9110#section-15.5.2",
        HttpStatusCode.Forbidden => "https://tools.ietf.org/html/rfc9110#section-15.5.4",
        HttpStatusCode.InternalServerError => "https://tools.ietf.org/html/rfc9110#section-15.6.1",
        _ => "about:blank"
    };
}
