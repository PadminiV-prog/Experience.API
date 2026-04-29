using Experience.API.Configuration;
using Experience.API.Contract;
using Microsoft.Extensions.Logging;

namespace Experience.API.TransformAdapter
{
    public class ExperienceTransformAdapter : ITransformAdapter
    {
        private readonly AppSettings _appSettings;
        private readonly ILogger<ExperienceTransformAdapter> _logger;

        public ExperienceTransformAdapter(AppSettings appSettings, ILogger<ExperienceTransformAdapter> logger)
        {
            _appSettings = appSettings;
            _logger = logger;
        }

        public Task<string> TransformRequestAsync(string payload, string correlationId)
        {
            _logger.LogInformation("Transforming request. CorrelationId: {CorrelationId}", correlationId);
            return Task.FromResult(payload);
        }

        public Task<string> TransformResponseAsync(string payload, string correlationId)
        {
            _logger.LogInformation("Transforming response. CorrelationId: {CorrelationId}", correlationId);
            return Task.FromResult(payload);
        }
    }
}
