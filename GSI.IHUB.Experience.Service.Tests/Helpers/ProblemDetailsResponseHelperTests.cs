using GSI.IHUB.Experience.Service.Helpers;
using GSI.IHUB.Experience.Service.Model;
using GSI.IHUB.Experience.Service.Tests.Setup;
using Newtonsoft.Json;
using System.Net;

namespace GSI.IHUB.Experience.Service.Tests.Helpers;

public class ProblemDetailsResponseHelperTests
{
    // -------------------------------------------------------------------------
    // GetProblemType
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "https://tools.ietf.org/html/rfc9110#section-15.5.1")]
    [InlineData(HttpStatusCode.Unauthorized, "https://tools.ietf.org/html/rfc9110#section-15.5.2")]
    [InlineData(HttpStatusCode.Forbidden, "https://tools.ietf.org/html/rfc9110#section-15.5.4")]
    [InlineData(HttpStatusCode.InternalServerError, "https://tools.ietf.org/html/rfc9110#section-15.6.1")]
    public void GetProblemType_KnownStatusCode_ReturnsExpectedUri(HttpStatusCode statusCode, string expectedUri)
    {
        var result = ProblemDetailsResponseHelper.GetProblemType(statusCode);
        Assert.Equal(expectedUri, result);
    }

    [Fact]
    public void GetProblemType_UnknownStatusCode_ReturnsAboutBlank()
    {
        var result = ProblemDetailsResponseHelper.GetProblemType(HttpStatusCode.NotFound);
        Assert.Equal("about:blank", result);
    }

    // -------------------------------------------------------------------------
    // CreateProblemResponseAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateProblemResponseAsync_SetsStatusCode()
    {
        var fixture = new TestFixture();
        var request = fixture.CreateHttpRequest(string.Empty);

        var response = await ProblemDetailsResponseHelper.CreateProblemResponseAsync(
            request, HttpStatusCode.BadRequest, "Bad Request", "detail", "corr-1");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateProblemResponseAsync_SetsContentTypeHeader()
    {
        var fixture = new TestFixture();
        var request = fixture.CreateHttpRequest(string.Empty);

        var response = await ProblemDetailsResponseHelper.CreateProblemResponseAsync(
            request, HttpStatusCode.BadRequest, "Bad Request", "detail", "corr-1");

        Assert.True(response.Headers.TryGetValues("Content-Type", out var values));
        Assert.Contains(values, v => v.Contains("application/problem+json"));
    }

    [Fact]
    public async Task CreateProblemResponseAsync_BodyContainsCorrectProblemDetails()
    {
        const string correlationId = "corr-xyz";
        var fixture = new TestFixture();
        var request = fixture.CreateHttpRequest(string.Empty);

        var response = await ProblemDetailsResponseHelper.CreateProblemResponseAsync(
            request, HttpStatusCode.InternalServerError, "Internal Server Error", "Something failed.", correlationId);

        var problem = ReadProblemDetails(response);
        Assert.Equal(500, problem.Status);
        Assert.Equal("Internal Server Error", problem.Title);
        Assert.Equal("Something failed.", problem.Detail);
        Assert.Equal(correlationId, problem.CorrelationId);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.6.1", problem.Type);
        Assert.False(string.IsNullOrWhiteSpace(problem.Instance));
    }

    [Fact]
    public async Task CreateProblemResponseAsync_UsesGetProblemType_ForBadRequest()
    {
        var fixture = new TestFixture();
        var request = fixture.CreateHttpRequest(string.Empty);

        var response = await ProblemDetailsResponseHelper.CreateProblemResponseAsync(
            request, HttpStatusCode.BadRequest, "Bad Request", "invalid payload", "corr-2");

        var problem = ReadProblemDetails(response);
        Assert.Equal(ProblemDetailsResponseHelper.GetProblemType(HttpStatusCode.BadRequest), problem.Type);
    }

    // -------------------------------------------------------------------------
    // Helper
    // -------------------------------------------------------------------------

    private static ExperienceProblemDetails ReadProblemDetails(Microsoft.Azure.Functions.Worker.Http.HttpResponseData response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new System.IO.StreamReader(response.Body, System.Text.Encoding.UTF8, leaveOpen: true);
        return JsonConvert.DeserializeObject<ExperienceProblemDetails>(reader.ReadToEnd())!;
    }
}
