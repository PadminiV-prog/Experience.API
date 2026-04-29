using Experience.API.Model;

namespace Experience.API.Contract
{
    public interface IExperienceService
    {
        Task<ExperienceResponse> ProcessAsync(ExperienceRequest request);
    }
}
