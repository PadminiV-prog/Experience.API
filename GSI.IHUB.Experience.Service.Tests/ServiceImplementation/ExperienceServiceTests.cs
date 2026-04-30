using GSI.IHUB.Experience.Service.Contracts;
using GSI.IHUB.Experience.Service.Model;
using GSI.IHUB.Experience.Service.ServiceImplementation;
using Microsoft.Extensions.Options;

namespace GSI.IHUB.Experience.Service.Tests.ServiceImplementation;

public class ExperienceServiceTests
{
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IHttpClientService> _httpClientServiceMock = new();
    private readonly AppSettings _appSettings = new()
    {
        SystemApiBaseUrl = "https://api.example.com",
        SystemApiVersion = "v1",
        SystemApiSubscriptionKey = "sub-key",
        B2BClientId = "client-id",
        B2BClientSecret = "client-secret",
        B2BTenantAuthorityUrl = "https://login.microsoftonline.com/tenant",
        CacheKeySystemApi = "MyCache"
    };

    private ExperienceService CreateSut()
    {
        var options = Options.Create(_appSettings);
        return new ExperienceService(options, _tokenServiceMock.Object, _httpClientServiceMock.Object);
    }

    [Fact]
    public async Task ProcessRequestAsync_ValidRequest_ReturnsServiceResponse()
    {
        var sut = CreateSut();
        var request = new ExperienceRequest { Route = "orders/process", Payload = new { id = 1 } };
        const string correlationId = "corr-123";
        const string expectedResponse = "{\"status\":\"ok\"}";

        _tokenServiceMock
            .Setup(x => x.GetTokenAsync(It.IsAny<ClientRequestModel>()))
            .ReturnsAsync("bearer-token");

        _httpClientServiceMock
            .Setup(x => x.PostAsync(It.IsAny<ClientRequestModel>(), "bearer-token", correlationId))
            .ReturnsAsync(expectedResponse);

        var result = await sut.ProcessRequestAsync(request, correlationId);

        Assert.Equal(expectedResponse, result);
    }

    [Fact]
    public async Task ProcessRequestAsync_EmptyDownstreamResponse_ReturnsNoResponse()
    {
        var sut = CreateSut();
        var request = new ExperienceRequest { Route = "orders/process", Payload = new { id = 1 } };

        _tokenServiceMock
            .Setup(x => x.GetTokenAsync(It.IsAny<ClientRequestModel>()))
            .ReturnsAsync("bearer-token");

        _httpClientServiceMock
            .Setup(x => x.PostAsync(It.IsAny<ClientRequestModel>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(string.Empty);

        var result = await sut.ProcessRequestAsync(request, "corr-456");

        Assert.Equal("NoResponse", result);
    }

    [Fact]
    public async Task ProcessRequestAsync_TokenServiceThrows_PropagatesException()
    {
        var sut = CreateSut();
        var request = new ExperienceRequest { Route = "orders/process", Payload = new { id = 1 } };

        _tokenServiceMock
            .Setup(x => x.GetTokenAsync(It.IsAny<ClientRequestModel>()))
            .ThrowsAsync(new InvalidOperationException("token error"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.ProcessRequestAsync(request, "corr-789"));
    }

    [Fact]
    public async Task ProcessRequestAsync_HttpClientThrows_PropagatesException()
    {
        var sut = CreateSut();
        var request = new ExperienceRequest { Route = "orders/process", Payload = new { id = 1 } };

        _tokenServiceMock
            .Setup(x => x.GetTokenAsync(It.IsAny<ClientRequestModel>()))
            .ReturnsAsync("bearer-token");

        _httpClientServiceMock
            .Setup(x => x.PostAsync(It.IsAny<ClientRequestModel>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new HttpRequestException("downstream error"));

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            sut.ProcessRequestAsync(request, "corr-000"));
    }
}
