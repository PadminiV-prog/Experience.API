using Microsoft.Extensions.Logging;
using Moq;
using Experience.API.Contract;
using Experience.API.Helpers;
using Experience.API.Model;
using Experience.API.ServiceImplementation;
using Xunit;

namespace Experience.API.Tests.ServiceImplementation
{
    public class ExperienceServiceTests
    {
        private readonly Mock<IHttpClientService> _mockHttpClientService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IValidationService> _mockValidationService;
        private readonly Mock<ITransformAdapter> _mockTransformAdapter;
        private readonly Mock<ILogger<ExperienceService>> _mockLogger;
        private readonly ExperienceService _service;

        public ExperienceServiceTests()
        {
            _mockHttpClientService = new Mock<IHttpClientService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockValidationService = new Mock<IValidationService>();
            _mockTransformAdapter = new Mock<ITransformAdapter>();
            _mockLogger = new Mock<ILogger<ExperienceService>>();

            _service = new ExperienceService(
                _mockHttpClientService.Object,
                _mockTokenService.Object,
                _mockValidationService.Object,
                _mockTransformAdapter.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task ProcessAsync_ValidRequest_ReturnsSuccessResponse()
        {
            var request = new ExperienceRequest { CorrelationId = "corr-001", Payload = "{}" };
            const string token = "access-token";
            const string transformedRequest = "{\"transformed\":true}";
            const string downstreamResponse = "{\"status\":\"ok\"}";
            const string transformedResponse = "{\"status\":\"ok\"}";
            string? outMessage;

            _mockValidationService
                .Setup(v => v.ValidateRequest(request, out outMessage))
                .Returns(true);
            _mockTokenService.Setup(t => t.GetAccessTokenAsync()).ReturnsAsync(token);
            _mockTransformAdapter
                .Setup(a => a.TransformRequestAsync(request.Payload, request.CorrelationId))
                .ReturnsAsync(transformedRequest);
            _mockHttpClientService
                .Setup(h => h.PostAsync(It.IsAny<string>(), transformedRequest, token, request.CorrelationId))
                .ReturnsAsync(downstreamResponse);
            _mockTransformAdapter
                .Setup(a => a.TransformResponseAsync(downstreamResponse, request.CorrelationId))
                .ReturnsAsync(transformedResponse);

            var result = await _service.ProcessAsync(request);

            Assert.True(result.IsSuccess);
            Assert.Equal("corr-001", result.CorrelationId);
            Assert.Equal(transformedResponse, result.Data);
        }

        [Fact]
        public async Task ProcessAsync_InvalidRequest_ReturnsFailureResponse()
        {
            var request = new ExperienceRequest { CorrelationId = "", Payload = "" };
            var validationMessage = "CorrelationId is required.";

            _mockValidationService
                .Setup(v => v.ValidateRequest(request, out validationMessage))
                .Returns(false);

            var result = await _service.ProcessAsync(request);

            Assert.False(result.IsSuccess);
            Assert.Equal(validationMessage, result.Message);
            _mockTokenService.Verify(t => t.GetAccessTokenAsync(), Times.Never);
        }

        [Fact]
        public async Task ProcessAsync_TokenServiceThrows_PropagatesException()
        {
            var request = new ExperienceRequest { CorrelationId = "corr-001", Payload = "{}" };
            string? outMessage;

            _mockValidationService
                .Setup(v => v.ValidateRequest(request, out outMessage))
                .Returns(true);
            _mockTokenService
                .Setup(t => t.GetAccessTokenAsync())
                .ThrowsAsync(new HttpRequestException("Token endpoint unavailable"));

            await Assert.ThrowsAsync<HttpRequestException>(() => _service.ProcessAsync(request));
        }
    }
}
