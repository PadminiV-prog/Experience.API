using GSI.IHUB.Experience.Service.Helpers;
using GSI.IHUB.Experience.Service.Model;
using System.Net;
using System.Net.Http.Headers;

namespace GSI.IHUB.Experience.Service.Tests.Helpers;

public class HttpClientServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();

    private HttpClientService CreateSut(HttpClient httpClient)
    {
        _httpClientFactoryMock
            .Setup(x => x.CreateClient("ExperienceServiceHttpClient"))
            .Returns(httpClient);
        return new HttpClientService(_httpClientFactoryMock.Object);
    }

    [Fact]
    public async Task PostAsync_SuccessfulResponse_ReturnsContent()
    {
        const string expectedBody = "{\"result\":\"ok\"}";
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, expectedBody);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.com") };
        var sut = CreateSut(httpClient);

        var model = new ClientRequestModel
        {
            BaseUrl = "https://api.example.com",
            Url = "orders/process",
            ApiVersion = "v1",
            SubscriptionKey = "sub-key",
            Data = "{\"id\":1}"
        };

        var result = await sut.PostAsync(model, "bearer-token", "corr-123");

        Assert.Equal(expectedBody, result);
    }

    [Fact]
    public async Task PostAsync_FailureStatusCode_ThrowsHttpRequestException()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.BadRequest, "bad request");
        var httpClient = new HttpClient(handler);
        var sut = CreateSut(httpClient);

        var model = new ClientRequestModel
        {
            BaseUrl = "https://api.example.com",
            Url = "orders/process",
            ApiVersion = string.Empty,
            SubscriptionKey = "sub-key",
            Data = "{\"id\":1}"
        };

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            sut.PostAsync(model, "bearer-token", "corr-456"));
    }

    [Fact]
    public async Task PostAsync_NoApiVersion_BuildsUriWithoutQueryString()
    {
        const string expectedBody = "{}";
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, expectedBody);
        var httpClient = new HttpClient(handler);
        var sut = CreateSut(httpClient);

        var model = new ClientRequestModel
        {
            BaseUrl = "https://api.example.com",
            Url = "orders/process",
            ApiVersion = string.Empty,
            SubscriptionKey = "sub-key",
            Data = "{}"
        };

        var result = await sut.PostAsync(model, "bearer-token", "corr-789");

        Assert.Equal(expectedBody, result);
        Assert.DoesNotContain("api-version", handler.LastRequestUri?.ToString() ?? string.Empty);
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _content;
        public Uri? LastRequestUri { get; private set; }

        public FakeHttpMessageHandler(HttpStatusCode statusCode, string content)
        {
            _statusCode = statusCode;
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri;
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_content)
            };
            return Task.FromResult(response);
        }
    }
}
