using GSI.IHUB.Experience.Service.Model;

namespace GSI.IHUB.Experience.Service.Contracts;

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
    /// <returns>The response content from the downstream system.</returns>
    Task<string> ProcessRequestAsync(ExperienceRequest request, string correlationId);

    /// <summary>
    /// Reads a CSV file from Azure Blob storage.
    /// </summary>
    /// <param name="containerName">The blob container name.</param>
    /// <param name="blobName">The blob file name.</param>
    /// <returns>The CSV file content as a string.</returns>
    Task<string> ReadCsvFromBlobAsync(string containerName, string blobName);
}
