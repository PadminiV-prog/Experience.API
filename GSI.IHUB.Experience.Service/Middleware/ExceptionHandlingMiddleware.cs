using GSI.IHUB.Experience.Service.Helpers;
using GSI.IHUB.Experience.Service.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System.Net;

namespace GSI.IHUB.Experience.Service.Middleware;

/// <summary>
/// Outermost Azure Functions worker middleware that catches any unhandled exception thrown
/// during function execution and returns a standardised RFC 7807 ProblemDetails response
/// (application/problem+json, HTTP 500) to the caller.
/// </summary>
public class ExceptionHandlingMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var correlationId = context.Items.TryGetValue(ApplicationConstants.CorrelationIdHeaderKey, out var corr)
                ? corr?.ToString() ?? Guid.NewGuid().ToString()
                : Guid.NewGuid().ToString();

            _logger.LogError(ex, "Unhandled exception occurred. CorrelationId: {CorrelationId}", correlationId);

            HttpRequestData? requestData = null;
            try
            {
                requestData = await context.GetHttpRequestDataAsync();
            }
            catch
            {
                // Feature infrastructure is unavailable (non-HTTP trigger or test environment).
            }

            if (requestData is null)
            {
                return;
            }

            var response = await ProblemDetailsResponseHelper.CreateProblemResponseAsync(
                requestData,
                HttpStatusCode.InternalServerError,
                "Internal Server Error",
                "An internal server error occurred.",
                correlationId);

            context.GetInvocationResult().Value = response;
        }
    }
}
