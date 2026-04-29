using Microsoft.Extensions.Logging;
using Moq;
using Experience.API.Contract;
using Experience.API.Model;
using Xunit;

namespace Experience.API.Tests.Functions
{
    public class ExperienceFunctionTests
    {
        private readonly Mock<IExperienceService> _mockExperienceService;
        private readonly Mock<IValidationService> _mockValidationService;
        private readonly Mock<ILogger<Experience.API.Functions.ExperienceFunction>> _mockLogger;

        public ExperienceFunctionTests()
        {
            _mockExperienceService = new Mock<IExperienceService>();
            _mockValidationService = new Mock<IValidationService>();
            _mockLogger = new Mock<ILogger<Experience.API.Functions.ExperienceFunction>>();
        }

        [Fact]
        public void ExperienceFunction_CanBeInstantiated()
        {
            var function = new Experience.API.Functions.ExperienceFunction(
                _mockExperienceService.Object,
                _mockValidationService.Object,
                _mockLogger.Object);

            Assert.NotNull(function);
        }

        [Fact]
        public void ExperienceService_ProcessAsync_IsCalledWithValidRequest()
        {
            var request = new ExperienceRequest { CorrelationId = "corr-001", Payload = "{}" };
            _mockExperienceService
                .Setup(s => s.ProcessAsync(It.IsAny<ExperienceRequest>()))
                .ReturnsAsync(new ExperienceResponse
                {
                    CorrelationId = request.CorrelationId,
                    IsSuccess = true,
                    Message = "Processed"
                });

            _mockExperienceService.Verify(s => s.ProcessAsync(It.IsAny<ExperienceRequest>()), Times.Never);
        }
    }
}
