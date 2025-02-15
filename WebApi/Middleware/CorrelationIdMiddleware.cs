using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SavingsAccount.Middleware;

public class CorrelationIdMiddleware : IMiddleware
{
    private readonly string CorrelationIdHeader = "X-Correlation-ID";
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    
    public CorrelationIdMiddleware(ILogger<CorrelationIdMiddleware> logger)
    {
        _logger = logger;
    }


    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        _logger.LogDebug("Correlation Id is executing.......");
        string? correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault();

        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
            context.Request.Headers[CorrelationIdHeader] = correlationId; 
        }

        // Ensure the correlation ID is also included in the response
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        using var logScope = _logger.BeginScope(new Dictionary<string, object?>
        {
            { CorrelationIdHeader, correlationId }
        });
        
        _logger.LogDebug("Correlation Id {0} is executed successfully....", correlationId);
        await next(context);

    }
}