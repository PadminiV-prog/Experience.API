using Experience.API.Model;

namespace Experience.API.Contract
{
    public interface IValidationService
    {
        bool ValidateRequest(ExperienceRequest request, out string validationMessage);
    }
}
