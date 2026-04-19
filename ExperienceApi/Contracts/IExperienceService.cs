using ExperienceApi.Model;

namespace ExperienceApi.Contracts;

/// <summary>
/// Processes incoming Experience API requests.
/// </summary>
public interface IExperienceService
{
    /// <summary>
    /// Processes an experience request and returns the downstream response content.
    /// </summary>
    /// <param name="request">The request payload.</param>
    /// <param name="correlationId">The correlation identifier.</param>
    /// <param name="sourceId">The source identifier.</param>
    /// <returns>The response content from the downstream system.</returns>
    Task<string> ProcessRequestAsync(ExperienceRequest request, string correlationId, string sourceId);
}
