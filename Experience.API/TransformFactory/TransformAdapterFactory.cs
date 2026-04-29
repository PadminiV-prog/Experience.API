using Experience.API.Contract;
using Experience.API.TransformAdapter;
using Microsoft.Extensions.DependencyInjection;

namespace Experience.API.TransformFactory
{
    public static class TransformAdapterFactory
    {
        public static ITransformAdapter Create(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<ExperienceTransformAdapter>();
        }
    }
}
