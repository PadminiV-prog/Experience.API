using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Security.Claims;

namespace ExperienceApi.Tests.Setup;

public sealed class TestFixture
{
    public Mock<ILogger<ExperienceApi.Functions.ProcessRequestFunction>> LoggerMock { get; } = new();

    public Mock<ExperienceApi.Contracts.IExperienceService> ExperienceServiceMock { get; } = new();

    public Mock<ExperienceApi.Contracts.IValidationService> ValidationServiceMock { get; } = new();

    public FunctionContext FunctionContext { get; }

    public TestFixture()
    {
        var contextItems = new Dictionary<object, object>();
        var contextMock = new Mock<FunctionContext>();
        contextMock.Setup(c => c.Items).Returns(contextItems);
        FunctionContext = contextMock.Object;
    }

    public HttpRequestData CreateHttpRequest(string body)
    {
        var request = new FakeHttpRequestData(FunctionContext, body);
        request.Headers.Add("x-correlation-id", "test-correlation-id");
        request.Headers.Add("x-source-id", "DC");
        return request;
    }

    private sealed class FakeHttpRequestData : HttpRequestData
    {
        public FakeHttpRequestData(FunctionContext functionContext, string body)
            : base(functionContext)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(body);
            Body = new MemoryStream(bytes);
            Headers = new HttpHeadersCollection();
            Url = new Uri("https://localhost/experience/process");
            Identities = Array.Empty<ClaimsIdentity>();
            Method = "POST";
        }

        public override Stream Body { get; }

        public override HttpHeadersCollection Headers { get; }

        public override IReadOnlyCollection<IHttpCookie> Cookies => Array.Empty<IHttpCookie>();

        public override Uri Url { get; }

        public override IEnumerable<ClaimsIdentity> Identities { get; }

        public override string Method { get; }

        public override HttpResponseData CreateResponse()
        {
            return new FakeHttpResponseData(FunctionContext);
        }
    }

    private sealed class FakeHttpResponseData : HttpResponseData
    {
        public FakeHttpResponseData(FunctionContext functionContext)
            : base(functionContext)
        {
            Headers = new HttpHeadersCollection();
            Body = new MemoryStream();
            Cookies = new Mock<HttpCookies>().Object;
            StatusCode = HttpStatusCode.OK;
        }

        public override HttpStatusCode StatusCode { get; set; }

        public override HttpHeadersCollection Headers { get; set; }

        public override Stream Body { get; set; }

        public override HttpCookies Cookies { get; }
    }
}
