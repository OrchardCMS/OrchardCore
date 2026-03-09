using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace OrchardCore.OpenApi;

/// <summary>
/// Diagnostic middleware to log authentication and authorization details.
/// Helps debug why API requests are returning 401.
/// </summary>
public sealed class AuthenticationDiagnosticsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationDiagnosticsMiddleware> _logger;

    public AuthenticationDiagnosticsMiddleware(RequestDelegate next, ILogger<AuthenticationDiagnosticsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Log incoming request
        var authHeader = context.Request.Headers.Authorization.ToString();
        _logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");
        _logger.LogInformation($"Authorization header: {(string.IsNullOrEmpty(authHeader) ? "[none]" : authHeader.Substring(0, Math.Min(50, authHeader.Length)) + "...")}");

        // Call next middleware
        await _next(context);

        // Log after authentication/authorization
        _logger.LogInformation($"Response status: {context.Response.StatusCode}");
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            _logger.LogInformation($"Authenticated as: {context.User.Identity.Name ?? "[unknown]"}");
            var roles = string.Join(", ", context.User.FindAll("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                .Select(c => c.Value));
            if (!string.IsNullOrEmpty(roles))
            {
                _logger.LogInformation($"Roles: {roles}");
            }
        }
        else
        {
            _logger.LogInformation("Not authenticated");
        }
    }
}
