using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace Experience.API.Middleware
{
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
                _logger.LogError(ex, "Unhandled exception in function {FunctionName}", context.FunctionDefinition.Name);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(FunctionContext context, Exception exception)
        {
            var httpRequestData = await context.GetHttpRequestDataAsync();
            if (httpRequestData == null)
            {
                return;
            }

            var response = httpRequestData.CreateResponse(HttpStatusCode.InternalServerError);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            var errorBody = JsonSerializer.Serialize(new
            {
                IsSuccess = false,
                Message = "An unexpected error occurred. Please try again or contact support.",
                Error = exception.Message
            });

            await response.WriteStringAsync(errorBody);

            var invocationResult = context.GetInvocationResult();
            invocationResult.Value = response;
        }
    }
}
