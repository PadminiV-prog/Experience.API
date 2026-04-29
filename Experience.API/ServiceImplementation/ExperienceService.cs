using Experience.API.Contract;
using Experience.API.Model;
using Microsoft.Extensions.Logging;

namespace Experience.API.ServiceImplementation
{
    public class ExperienceService : IExperienceService
    {
        private readonly IHttpClientService _httpClientService;
        private readonly ITokenService _tokenService;
        private readonly IValidationService _validationService;
        private readonly ILogger<ExperienceService> _logger;

        public ExperienceService(
            IHttpClientService httpClientService,
            ITokenService tokenService,
            IValidationService validationService,
            ILogger<ExperienceService> logger)
        {
            _httpClientService = httpClientService;
            _tokenService = tokenService;
            _validationService = validationService;
            _logger = logger;
        }

        public async Task<ExperienceResponse> ProcessAsync(ExperienceRequest request)
        {
            _logger.LogInformation("Processing request. CorrelationId: {CorrelationId}", request.CorrelationId);

            if (!_validationService.ValidateRequest(request, out var validationMessage))
            {
                _logger.LogWarning("Request validation failed. CorrelationId: {CorrelationId} | Reason: {Reason}",
                    request.CorrelationId, validationMessage);

                return new ExperienceResponse
                {
                    CorrelationId = request.CorrelationId,
                    IsSuccess = false,
                    Message = validationMessage
                };
            }

            var accessToken = await _tokenService.GetAccessTokenAsync();

            _logger.LogInformation("Calling downstream service. CorrelationId: {CorrelationId}", request.CorrelationId);
            var downstreamResponse = await _httpClientService.PostAsync(
                "/api/process",
                request.Payload,
                accessToken,
                request.CorrelationId);

            return new ExperienceResponse
            {
                CorrelationId = request.CorrelationId,
                IsSuccess = true,
                Message = "Request processed successfully.",
                Data = downstreamResponse
            };
        }
    }
}
