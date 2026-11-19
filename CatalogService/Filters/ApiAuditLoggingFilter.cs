using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace CatalogService.Filters;

/// <summary>
/// Єдина точка логування результатів API: хто, яку дію виконав і чим усе закінчилося.
/// Помилкові відповіді (4xx) логуються як Warning, успішні зміни даних (POST/PUT/DELETE) - як Information.
/// Паролі, токени та тіла запитів у лог не потрапляють.
/// </summary>
public class ApiAuditLoggingFilter : IAlwaysRunResultFilter
{
    private readonly ILogger<ApiAuditLoggingFilter> _logger;

    public ApiAuditLoggingFilter(ILogger<ApiAuditLoggingFilter> logger)
    {
        _logger = logger;
    }

    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor descriptor)
            return;

        if (descriptor.ControllerName == "Auth")
            return;

        var statusCode = (context.Result as IStatusCodeActionResult)?.StatusCode;
        if (statusCode == null)
            return;

        var action = $"{descriptor.ControllerName}.{descriptor.ActionName}";
        var httpUser = context.HttpContext.User;
        var user = httpUser.Identity?.IsAuthenticated == true ? httpUser.Identity.Name ?? "unknown" : "anonymous";
        var roles = string.Join(",", httpUser.FindAll("role").Select(c => c.Value));

        if (statusCode >= 400)
        {
            _logger.LogWarning("{Action} by {User} [{Roles}] -> {StatusCode}: {Reason}",
                action, user, roles, statusCode, Describe(context.Result));
            return;
        }

        var method = context.HttpContext.Request.Method;
        if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method))
            return;

        _logger.LogInformation("{Action} by {User} [{Roles}] -> {StatusCode}, entity id: {EntityId}",
            action, user, roles, statusCode, GetEntityId(context));
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }

    private static string Describe(IActionResult result) => result switch
    {
        ObjectResult { Value: ValidationProblemDetails details } =>
            $"validation failed for: {string.Join(", ", details.Errors.Keys)}",
        ObjectResult { Value: not null } objectResult => objectResult.Value.ToString() ?? string.Empty,
        _ => string.Empty
    };

    private static object GetEntityId(ResultExecutingContext context)
    {
        if (context.RouteData.Values.TryGetValue("id", out var routeId) && routeId != null)
            return routeId;

        return (context.Result as CreatedAtActionResult)?.RouteValues?["id"] ?? "n/a";
    }
}