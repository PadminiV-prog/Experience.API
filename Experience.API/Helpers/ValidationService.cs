using Experience.API.Model;
using Microsoft.Extensions.Logging;

namespace Experience.API.Helpers
{
    public class ValidationService : IValidationService
    {
        private readonly ILogger<ValidationService> _logger;

        public ValidationService(ILogger<ValidationService> logger)
        {
            _logger = logger;
        }

        public bool ValidateRequest(ExperienceRequest request, out string validationMessage)
        {
            if (request == null)
            {
                validationMessage = "Request cannot be null.";
                _logger.LogWarning("Validation failed: request is null.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.CorrelationId))
            {
                validationMessage = "CorrelationId is required.";
                _logger.LogWarning("Validation failed: CorrelationId is missing.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.Payload))
            {
                validationMessage = "Payload is required.";
                _logger.LogWarning("Validation failed: Payload is missing. CorrelationId: {CorrelationId}", request.CorrelationId);
                return false;
            }

            validationMessage = string.Empty;
            return true;
        }
    }
}
