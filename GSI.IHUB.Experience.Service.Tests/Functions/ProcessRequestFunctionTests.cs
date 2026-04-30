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
    public async Task ProcessRequest_EmptyBody_ReturnsBadRequest()
    {
        var fixture = new TestFixture();
        var function = new ProcessRequestFunction(
            fixture.LoggerMock.Object,
            fixture.ExperienceServiceMock.Object,
            fixture.ValidationServiceMock.Object);

        var request = fixture.CreateHttpRequest(string.Empty);

        fixture.ValidationServiceMock
            .Setup(x => x.ValidateRequest(string.Empty))
            .Returns(false);

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
    }
}
