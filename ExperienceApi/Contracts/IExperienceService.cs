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

    /// <summary>
    /// Reads a CSV file from Azure Blob storage.
    /// </summary>
    /// <param name="containerName">The blob container name.</param>
    /// <param name="blobName">The blob file name.</param>
    /// <returns>The CSV file content as a string.</returns>
    Task<string> ReadCsvFromBlobAsync(string containerName, string blobName);
}
