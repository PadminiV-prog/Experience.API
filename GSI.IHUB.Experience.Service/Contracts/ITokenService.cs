using GSI.IHUB.Experience.Service.Model;

namespace GSI.IHUB.Experience.Service.Contracts;

/// <summary>
/// Provides access tokens for downstream API calls.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Gets an OAuth token for the specified client request model.
    /// </summary>
    /// <param name="model">Client request details.</param>
    /// <returns>A bearer token string.</returns>
    Task<string> GetTokenAsync(ClientRequestModel model);
}
