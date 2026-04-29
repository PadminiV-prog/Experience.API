using System.Net;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Experience.API.Configuration;
using Experience.API.Helpers;
using Xunit;

namespace Experience.API.Tests.Helpers
{
    public class TokenServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHandler;
        private readonly AppSettings _appSettings;
        private readonly Mock<ILogger<TokenService>> _mockLogger;
        private readonly TokenService _service;

        public TokenServiceTests()
        {
            _mockHandler = new Mock<HttpMessageHandler>();
            _mockLogger = new Mock<ILogger<TokenService>>();
            _appSettings = new AppSettings
            {
                TokenEndpoint = "https://auth.example.com/token",
                ClientId = "client-id",
                ClientSecret = "client-secret",
                Scope = "api://scope/.default"
            };
            var httpClient = new HttpClient(_mockHandler.Object);
            _service = new TokenService(httpClient, _appSettings, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAccessTokenAsync_ValidResponse_ReturnsToken()
        {
            const string expectedToken = "eyJhbGciOiJSUzI1NiJ9.test";
            var tokenResponse = JsonSerializer.Serialize(new
            {
                access_token = expectedToken,
                expires_in = 3600,
                token_type = "Bearer"
            });

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(tokenResponse)
                });

            var token = await _service.GetAccessTokenAsync();

            Assert.Equal(expectedToken, token);
        }

        [Fact]
        public async Task GetAccessTokenAsync_CachedToken_DoesNotCallEndpointAgain()
        {
            const string expectedToken = "cached-token";
            var tokenResponse = JsonSerializer.Serialize(new
            {
                access_token = expectedToken,
                expires_in = 3600,
                token_type = "Bearer"
            });

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(tokenResponse)
                });

            var firstToken = await _service.GetAccessTokenAsync();
            var secondToken = await _service.GetAccessTokenAsync();

            Assert.Equal(firstToken, secondToken);
            _mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetAccessTokenAsync_TokenEndpointFails_ThrowsHttpRequestException()
        {
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Content = new StringContent("Unauthorized")
                });

            await Assert.ThrowsAsync<HttpRequestException>(() => _service.GetAccessTokenAsync());
        }
    }
}
