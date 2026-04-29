using Experience.API.Model;

namespace Experience.API.Helpers
{
    public interface IValidationService
    {
        bool ValidateRequest(ExperienceRequest request, out string validationMessage);
    }
}
