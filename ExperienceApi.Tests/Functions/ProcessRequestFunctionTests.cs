using ExperienceApi.Functions;
using ExperienceApi.Model;
using ExperienceApi.Tests.Setup;
using Newtonsoft.Json;
using System.Net;

namespace ExperienceApi.Tests.Functions;

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
        var request = fixture.CreateHttpRequest(JsonConvert.SerializeObject(requestPayload));
        fixture.FunctionContext.Items[ApplicationConstants.CorrelationIdHeaderKey] = "corr-1";
        fixture.FunctionContext.Items[ApplicationConstants.SourceIdHeaderKey] = "DC";

        fixture.ValidationServiceMock
            .Setup(x => x.ValidateRequestAsync(It.IsAny<ExperienceRequest>()))
            .ReturnsAsync(true);

        fixture.ExperienceServiceMock
            .Setup(x => x.ProcessRequestAsync(It.IsAny<ExperienceRequest>(), "corr-1", "DC"))
            .ReturnsAsync("{\"status\":\"ok\"}");

        var response = await function.Run(request, fixture.FunctionContext);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ProcessRequest_EmptyBody_ReturnsBadRequest()
    {
        var fixture = new TestFixture();
        var function = new ProcessRequestFunction(
            fixture.LoggerMock.Object,
            fixture.ExperienceServiceMock.Object,
            fixture.ValidationServiceMock.Object);

        var request = fixture.CreateHttpRequest(string.Empty);

        var response = await function.Run(request, fixture.FunctionContext);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ProcessRequest_ServiceThrowsException_ReturnsInternalServerError()
    {
        var fixture = new TestFixture();
        var function = new ProcessRequestFunction(
            fixture.LoggerMock.Object,
            fixture.ExperienceServiceMock.Object,
            fixture.ValidationServiceMock.Object);

        var requestPayload = new ExperienceRequest { Route = "orders/process", Payload = new { orderId = "1" } };
        var request = fixture.CreateHttpRequest(JsonConvert.SerializeObject(requestPayload));
        fixture.FunctionContext.Items[ApplicationConstants.CorrelationIdHeaderKey] = "corr-1";
        fixture.FunctionContext.Items[ApplicationConstants.SourceIdHeaderKey] = "DC";

        fixture.ValidationServiceMock
            .Setup(x => x.ValidateRequestAsync(It.IsAny<ExperienceRequest>()))
            .ReturnsAsync(true);

        fixture.ExperienceServiceMock
            .Setup(x => x.ProcessRequestAsync(It.IsAny<ExperienceRequest>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("failure"));

        var response = await function.Run(request, fixture.FunctionContext);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}
