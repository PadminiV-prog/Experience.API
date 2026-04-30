using GSI.IHUB.Experience.Service.Model;

namespace GSI.IHUB.Experience.Service.Contracts;

/// <summary>
/// Validates incoming request payloads.
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validates the request payload.
    /// </summary>
    /// <param name="request">The incoming experience request.</param>
    /// <returns><c>true</c> when valid; otherwise, <c>false</c>.</returns>
    Task<bool> ValidateRequestAsync(ExperienceRequest request);
}
