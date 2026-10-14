using System.Diagnostics;

namespace RealEstateApi.Middleware;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;
    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        var watch = Stopwatch.StartNew();
        _logger.LogInformation("HTTP {RequestMethod} {RequestPath} started",
            context.Request.Method, context.Request.Path);
        await _next(context);
        watch.Stop();
        _logger.LogInformation("HTTP {RequestMethod} {RequestPath} completed with Status {StatusCode} in {ElapsedMilliseconds}ms",
            context.Request.Method, context.Request.Path, context.Response.StatusCode, watch.ElapsedMilliseconds);
    }
}