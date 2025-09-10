using EAI.Core.Models;

namespace EAI.Api.Middleware;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditMiddleware> _logger;

    public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        
        // Log request
        _logger.LogInformation("Request: {Method} {Path} from {RemoteIpAddress}", 
            context.Request.Method, 
            context.Request.Path, 
            context.Connection.RemoteIpAddress);

        await _next(context);

        var duration = DateTime.UtcNow - startTime;
        
        // Log response
        _logger.LogInformation("Response: {StatusCode} in {Duration}ms", 
            context.Response.StatusCode, 
            duration.TotalMilliseconds);
    }
}
