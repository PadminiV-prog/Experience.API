using System.Net;
using System.Text.Json;
using Experience.API.Contract;
using Experience.API.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Experience.API.Functions
{
    public class ExperienceFunction
    {
        private readonly IExperienceService _experienceService;
        private readonly IValidationService _validationService;
        private readonly ILogger<ExperienceFunction> _logger;

        public ExperienceFunction(
            IExperienceService experienceService,
            IValidationService validationService,
            ILogger<ExperienceFunction> logger)
        {
            _experienceService = experienceService;
            _validationService = validationService;
            _logger = logger;
        }

        [Function("ExperienceFunction")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "experience")] HttpRequestData req)
        {
            _logger.LogInformation("ExperienceFunction triggered.");

            var requestBody = await req.ReadAsStringAsync();
            ExperienceRequest? request;

            try
            {
                request = JsonSerializer.Deserialize<ExperienceRequest>(requestBody ?? string.Empty,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize request body.");
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteAsJsonAsync(new ExperienceResponse
                {
                    IsSuccess = false,
                    Message = "Invalid request format."
                });
                return badRequest;
            }

            if (request == null)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteAsJsonAsync(new ExperienceResponse
                {
                    IsSuccess = false,
                    Message = "Request body is required."
                });
                return badRequest;
            }

            if (!_validationService.ValidateRequest(request, out var validationMessage))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteAsJsonAsync(new ExperienceResponse
                {
                    CorrelationId = request.CorrelationId,
                    IsSuccess = false,
                    Message = validationMessage
                });
                return badRequest;
            }

            var result = await _experienceService.ProcessAsync(request);

            var response = req.CreateResponse(result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(result);
            return response;
        }
    }
}
