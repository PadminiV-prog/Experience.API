using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Experience.API.Helpers;
using Xunit;

namespace Experience.API.Tests.Helpers
{
    public class HttpClientServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHandler;
        private readonly Mock<ILogger<HttpClientService>> _mockLogger;
        private readonly HttpClientService _service;

        public HttpClientServiceTests()
        {
            _mockHandler = new Mock<HttpMessageHandler>();
            _mockLogger = new Mock<ILogger<HttpClientService>>();
            var httpClient = new HttpClient(_mockHandler.Object);
            _service = new HttpClientService(httpClient, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAsync_SuccessfulResponse_ReturnsContent()
        {
            const string expectedContent = "{\"data\":\"test\"}";
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(expectedContent)
                });

            var result = await _service.GetAsync("https://example.com/api", "token123", "corr-001");

            Assert.Equal(expectedContent, result);
        }

        [Fact]
        public async Task PostAsync_SuccessfulResponse_ReturnsContent()
        {
            const string expectedContent = "{\"result\":\"ok\"}";
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(expectedContent)
                });

            var result = await _service.PostAsync("https://example.com/api", "{}", "token123", "corr-001");

            Assert.Equal(expectedContent, result);
        }

        [Fact]
        public async Task PostAsync_UnsuccessfulResponse_ThrowsHttpRequestException()
        {
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent("Server error")
                });

            await Assert.ThrowsAsync<HttpRequestException>(
                () => _service.PostAsync("https://example.com/api", "{}", "token123", "corr-001"));
        }

        [Fact]
        public async Task DeleteAsync_SuccessfulResponse_ReturnsContent()
        {
            const string expectedContent = "{\"deleted\":true}";
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(expectedContent)
                });

            var result = await _service.DeleteAsync("https://example.com/api/1", "token123", "corr-001");

            Assert.Equal(expectedContent, result);
        }
    }
}
