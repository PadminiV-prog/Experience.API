using GSI.IHUB.Experience.Service.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

            var problem = new ExperienceProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                Title = "Internal Server Error",
                Status = (int)HttpStatusCode.InternalServerError,
                Detail = "An internal server error occurred.",
                Instance = requestData.Url.AbsolutePath,
                CorrelationId = correlationId
            };

            var response = requestData.CreateResponse(HttpStatusCode.InternalServerError);
            response.Headers.Add("Content-Type", "application/problem+json; charset=utf-8");
            await response.WriteStringAsync(JsonConvert.SerializeObject(problem));

            context.GetInvocationResult().Value = response;
        }
    }
}
