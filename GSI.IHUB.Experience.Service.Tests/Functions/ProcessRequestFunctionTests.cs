using GSI.IHUB.Experience.Service.Functions;
using GSI.IHUB.Experience.Service.Model;
using GSI.IHUB.Experience.Service.Tests.Setup;
using Newtonsoft.Json;
using System.Net;

namespace GSI.IHUB.Experience.Service.Tests.Functions;

public class ProcessRequestFunctionTests
{
    [Fact]
    public async Task ProcessRequest_ValidRequest_ReturnsOk()
    {
        var fixture = new TestFixture();
        var function = new ProcessRequestFunction(
            fixture.LoggerMock.Object,
            fixture.ExperienceServiceMock.Object,
            fixture.ValidationServiceMock.Object);

        var requestPayload = new ExperienceRequest { Route = "orders/process", Payload = new { orderId = "1" } };
        var requestBodyJson = JsonConvert.SerializeObject(requestPayload);
        var request = fixture.CreateHttpRequest(requestBodyJson);
        fixture.FunctionContext.Items[ApplicationConstants.CorrelationIdHeaderKey] = "corr-1";

        fixture.ValidationServiceMock
            .Setup(x => x.ValidateRequest(requestBodyJson))
            .Returns(true);

        fixture.ExperienceServiceMock
            .Setup(x => x.ProcessRequestAsync(It.IsAny<ExperienceRequest>(), "corr-1"))
            .ReturnsAsync("{\"status\":\"ok\"}");

        var response = await function.Run(request, fixture.FunctionContext);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ProcessRequest_EmptyBody_ReturnsBadRequest_WithProblemDetails()
    {
        var fixture = new TestFixture();
        var function = new ProcessRequestFunction(
            fixture.LoggerMock.Object,
            fixture.ExperienceServiceMock.Object,
            fixture.ValidationServiceMock.Object);

        var request = fixture.CreateHttpRequest(string.Empty);
        fixture.FunctionContext.Items[ApplicationConstants.CorrelationIdHeaderKey] = "corr-bad";

        fixture.ValidationServiceMock
            .Setup(x => x.ValidateRequest(string.Empty))
            .Returns(false);

        var response = await function.Run(request, fixture.FunctionContext);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = ReadProblemDetails(response);
        Assert.Equal(400, problem.Status);
        Assert.Equal("Bad Request", problem.Title);
        Assert.False(string.IsNullOrWhiteSpace(problem.Detail));
        Assert.Equal("corr-bad", problem.CorrelationId);
    }

    [Fact]
    public async Task ProcessRequest_ServiceThrowsException_ReturnsInternalServerError_WithProblemDetails()
    {
        var fixture = new TestFixture();
        var function = new ProcessRequestFunction(
            fixture.LoggerMock.Object,
            fixture.ExperienceServiceMock.Object,
            fixture.ValidationServiceMock.Object);

        var requestPayload = new ExperienceRequest { Route = "orders/process", Payload = new { orderId = "1" } };
        var requestBodyJson = JsonConvert.SerializeObject(requestPayload);
        var request = fixture.CreateHttpRequest(requestBodyJson);
        fixture.FunctionContext.Items[ApplicationConstants.CorrelationIdHeaderKey] = "corr-1";

        fixture.ValidationServiceMock
            .Setup(x => x.ValidateRequest(requestBodyJson))
            .Returns(true);

        fixture.ExperienceServiceMock
            .Setup(x => x.ProcessRequestAsync(It.IsAny<ExperienceRequest>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("failure"));

        var response = await function.Run(request, fixture.FunctionContext);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        var problem = ReadProblemDetails(response);
        Assert.Equal(500, problem.Status);
        Assert.Equal("Internal Server Error", problem.Title);
        Assert.False(string.IsNullOrWhiteSpace(problem.Detail));
        Assert.Equal("corr-1", problem.CorrelationId);
    }

    private static ExperienceProblemDetails ReadProblemDetails(Microsoft.Azure.Functions.Worker.Http.HttpResponseData response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new System.IO.StreamReader(response.Body, System.Text.Encoding.UTF8, leaveOpen: true);
        var json = reader.ReadToEnd();
        return JsonConvert.DeserializeObject<ExperienceProblemDetails>(json)!;
    }
}
