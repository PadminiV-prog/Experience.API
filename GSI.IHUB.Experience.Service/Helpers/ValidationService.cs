using GSI.IHUB.Experience.Service.Contracts;
using GSI.IHUB.Experience.Service.Model;

namespace GSI.IHUB.Experience.Service.Helpers;

public class ValidationService : IValidationService
{
    public Task<bool> ValidateRequestAsync(ExperienceRequest request)
    {
        var isValid = request is not null
                      && !string.IsNullOrWhiteSpace(request.Route)
                      && request.Payload is not null;

        return Task.FromResult(isValid);
    }
}
