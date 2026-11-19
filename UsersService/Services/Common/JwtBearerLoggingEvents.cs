using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace UsersService.Services.Common;

/// <summary>
/// Логування подій безпеки JWT-автентифікації.
/// </summary>
public static class JwtBearerLoggingEvents
{
    public static JwtBearerEvents Create() => new()
    {
        OnAuthenticationFailed = context =>
        {
            var logger = GetLogger(context.HttpContext);
            logger.LogWarning("JWT authentication failed for {Method} {Path}: {Reason}",
                context.HttpContext.Request.Method,
                context.HttpContext.Request.Path,
                context.Exception.GetType().Name);
            return Task.CompletedTask;
        },

        OnForbidden = context =>
        {
            var logger = GetLogger(context.HttpContext);
            logger.LogWarning("Access forbidden for {User} on {Method} {Path}",
                context.HttpContext.User.Identity?.Name ?? "unknown",
                context.HttpContext.Request.Method,
                context.HttpContext.Request.Path);
            return Task.CompletedTask;
        }
    };

    private static ILogger GetLogger(HttpContext httpContext) =>
        httpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Security");
}