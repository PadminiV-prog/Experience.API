namespace GSI.IHUB.Experience.Service.Contracts;

/// <summary>
/// Validates incoming request payloads.
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validates the raw request body string.
    /// </summary>
    /// <param name="requestBody">The raw JSON request body.</param>
    /// <returns><c>true</c> when valid; otherwise, <c>false</c>.</returns>
    bool ValidateRequest(string requestBody);
}
