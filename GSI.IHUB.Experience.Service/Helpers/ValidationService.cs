using GSI.IHUB.Experience.Service.Contracts;
using GSI.IHUB.Experience.Service.Model;
using Newtonsoft.Json;

namespace GSI.IHUB.Experience.Service.Helpers;

public class ValidationService : IValidationService
{
    public bool ValidateRequest(string requestBody)
    {
        if (string.IsNullOrWhiteSpace(requestBody) || requestBody.Trim() == "{}")
        {
            return false;
        }

        try
        {
            var request = JsonConvert.DeserializeObject<ExperienceRequest>(requestBody);

            return request is not null
                   && !string.IsNullOrWhiteSpace(request.Route)
                   && request.Payload is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
