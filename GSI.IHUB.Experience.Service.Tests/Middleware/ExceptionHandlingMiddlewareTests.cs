using GSI.IHUB.Experience.Service.Middleware;
using GSI.IHUB.Experience.Service.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace GSI.IHUB.Experience.Service.Tests.Middleware;

/// <summary>
/// Unit tests for <see cref="ExceptionHandlingMiddleware"/>.
///
/// Note: Because <c>IFunctionBindingsFeature</c> is an internal type in the Azure Functions
/// Worker SDK, the HTTP response-writing branch of the middleware (setting
/// <c>context.GetInvocationResult().Value</c>) cannot be exercised in isolation unit tests.
/// The tests below cover:
///   1. Happy path — <c>next</c> is invoked when no exception is thrown.
///   2. Exception path — an unhandled exception is caught, logged, and never re-thrown.
///   3. Correlation ID forwarding — the logged error message includes the correlation ID.
/// Full end-to-end response-body verification is covered by integration/acceptance tests
/// against the Azure Functions host.
/// </summary>
public class ExceptionHandlingMiddlewareTests
{
    // -------------------------------------------------------------------------
    // Helper
    // -------------------------------------------------------------------------

    private static (Mock<FunctionContext> ContextMock, Mock<ILogger<ExceptionHandlingMiddleware>> LoggerMock, ExceptionHandlingMiddleware Middleware)
        CreateSut(string correlationId = "corr-test")
    {
        var featuresMock = new Mock<IInvocationFeatures>();
        // IFunctionBindingsFeature is internal; Get<T>() returns default (null) by default
        // which causes GetHttpRequestDataAsync() to return null safely.

        var contextMock = new Mock<FunctionContext>();
        contextMock.Setup(c => c.Features).Returns(featuresMock.Object);
        contextMock.Setup(c => c.Items).Returns(new Dictionary<object, object>
        {
            [ApplicationConstants.CorrelationIdHeaderKey] = correlationId
        });

        var loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        var middleware = new ExceptionHandlingMiddleware(loggerMock.Object);

        return (contextMock, loggerMock, middleware);
    }

    // -------------------------------------------------------------------------
    // Tests
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Invoke_NoException_CallsNext()
    {
        var (contextMock, _, middleware) = CreateSut();
        var nextCalled = false;

        FunctionExecutionDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        await middleware.Invoke(contextMock.Object, next);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task Invoke_ExceptionThrown_DoesNotPropagateException()
    {
        var (contextMock, _, middleware) = CreateSut();

        FunctionExecutionDelegate next = _ => throw new InvalidOperationException("downstream failure");

        // Must NOT throw — the middleware is responsible for swallowing it
        var ex = await Record.ExceptionAsync(() => middleware.Invoke(contextMock.Object, next));
        Assert.Null(ex);
    }

    [Fact]
    public async Task Invoke_ExceptionThrown_LogsErrorWithCorrelationId()
    {
        const string correlationId = "corr-abc-123";
        var (contextMock, loggerMock, middleware) = CreateSut(correlationId);

        FunctionExecutionDelegate next = _ => throw new InvalidOperationException("test error");

        await middleware.Invoke(contextMock.Object, next);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains(correlationId)),
                It.IsAny<InvalidOperationException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Invoke_ExceptionThrown_MissingCorrelationId_DoesNotThrow()
    {
        var featuresMock = new Mock<IInvocationFeatures>();
        var contextMock = new Mock<FunctionContext>();
        contextMock.Setup(c => c.Features).Returns(featuresMock.Object);
        // Items has no correlation ID — middleware falls back to a new GUID
        contextMock.Setup(c => c.Items).Returns(new Dictionary<object, object>());

        var loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        var middleware = new ExceptionHandlingMiddleware(loggerMock.Object);

        FunctionExecutionDelegate next = _ => throw new Exception("error without correlation");

        var ex = await Record.ExceptionAsync(() => middleware.Invoke(contextMock.Object, next));
        Assert.Null(ex);
    }
}
