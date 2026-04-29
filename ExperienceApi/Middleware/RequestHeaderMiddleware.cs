using ExperienceApi.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace ExperienceApi.Middleware;

public class RequestHeaderMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var requestData = await context.GetHttpRequestDataAsync();

        var correlationId = GetHeaderValue(requestData, ApplicationConstants.CorrelationIdHeaderKey)
                            ?? Guid.NewGuid().ToString();

        context.Items[ApplicationConstants.CorrelationIdHeaderKey] = correlationId;

        await next(context);
    }

    private static string? GetHeaderValue(HttpRequestData? requestData, string headerName)
    {
        if (requestData is null)
        {
            return null;
        }

        return requestData.Headers.TryGetValues(headerName, out var values)
            ? values.FirstOrDefault()
            : null;
    }
}
