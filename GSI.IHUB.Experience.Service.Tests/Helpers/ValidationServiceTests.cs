using GSI.IHUB.Experience.Service.Helpers;
using GSI.IHUB.Experience.Service.Model;
using Newtonsoft.Json;

namespace GSI.IHUB.Experience.Service.Tests.Helpers;

public class ValidationServiceTests
{
    private readonly ValidationService _sut = new();

    [Fact]
    public void ValidateRequest_NullBody_ReturnsFalse()
    {
        var result = _sut.ValidateRequest(null!);

        Assert.False(result);
    }

    [Fact]
    public void ValidateRequest_EmptyBody_ReturnsFalse()
    {
        var result = _sut.ValidateRequest(string.Empty);

        Assert.False(result);
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("{}")]
    [InlineData("  {}  ")]
    public void ValidateRequest_WhitespaceOrEmptyJson_ReturnsFalse(string requestBody)
    {
        var result = _sut.ValidateRequest(requestBody);

        Assert.False(result);
    }

    [Fact]
    public void ValidateRequest_MissingRoute_ReturnsFalse()
    {
        var request = new ExperienceRequest { Route = string.Empty, Payload = new { id = 1 } };
        var json = JsonConvert.SerializeObject(request);

        var result = _sut.ValidateRequest(json);

        Assert.False(result);
    }

    [Fact]
    public void ValidateRequest_NullPayload_ReturnsFalse()
    {
        var request = new ExperienceRequest { Route = "orders/process", Payload = null };
        var json = JsonConvert.SerializeObject(request);

        var result = _sut.ValidateRequest(json);

        Assert.False(result);
    }

    [Fact]
    public void ValidateRequest_ValidRequest_ReturnsTrue()
    {
        var request = new ExperienceRequest { Route = "orders/process", Payload = new { id = 1 } };
        var json = JsonConvert.SerializeObject(request);

        var result = _sut.ValidateRequest(json);

        Assert.True(result);
    }

    [Fact]
    public void ValidateRequest_InvalidJson_ReturnsFalse()
    {
        var result = _sut.ValidateRequest("not-valid-json{{{");

        Assert.False(result);
    }
}
