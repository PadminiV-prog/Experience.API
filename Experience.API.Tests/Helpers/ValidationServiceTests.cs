using Microsoft.Extensions.Logging;
using Moq;
using Experience.API.Helpers;
using Experience.API.Model;
using Xunit;

namespace Experience.API.Tests.Helpers
{
    public class ValidationServiceTests
    {
        private readonly Mock<ILogger<ValidationService>> _mockLogger;
        private readonly ValidationService _service;

        public ValidationServiceTests()
        {
            _mockLogger = new Mock<ILogger<ValidationService>>();
            _service = new ValidationService(_mockLogger.Object);
        }

        [Fact]
        public void ValidateRequest_ValidRequest_ReturnsTrue()
        {
            var request = new ExperienceRequest
            {
                CorrelationId = "corr-001",
                Payload = "{\"key\":\"value\"}"
            };

            var result = _service.ValidateRequest(request, out var message);

            Assert.True(result);
            Assert.Empty(message);
        }

        [Fact]
        public void ValidateRequest_NullRequest_ReturnsFalse()
        {
            var result = _service.ValidateRequest(null!, out var message);

            Assert.False(result);
            Assert.Equal("Request cannot be null.", message);
        }

        [Fact]
        public void ValidateRequest_MissingCorrelationId_ReturnsFalse()
        {
            var request = new ExperienceRequest
            {
                CorrelationId = "",
                Payload = "{\"key\":\"value\"}"
            };

            var result = _service.ValidateRequest(request, out var message);

            Assert.False(result);
            Assert.Equal("CorrelationId is required.", message);
        }

        [Fact]
        public void ValidateRequest_MissingPayload_ReturnsFalse()
        {
            var request = new ExperienceRequest
            {
                CorrelationId = "corr-001",
                Payload = ""
            };

            var result = _service.ValidateRequest(request, out var message);

            Assert.False(result);
            Assert.Equal("Payload is required.", message);
        }

        [Fact]
        public void ValidateRequest_WhitespacePayload_ReturnsFalse()
        {
            var request = new ExperienceRequest
            {
                CorrelationId = "corr-001",
                Payload = "   "
            };

            var result = _service.ValidateRequest(request, out var message);

            Assert.False(result);
            Assert.Equal("Payload is required.", message);
        }
    }
}
