using ExperienceApi.Contracts;
using ExperienceApi.Model;

namespace ExperienceApi.Helpers;

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
