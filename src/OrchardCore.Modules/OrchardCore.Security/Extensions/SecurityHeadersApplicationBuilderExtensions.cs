using OrchardCore.Security.Options;
using OrchardCore.Security.Services;

namespace Microsoft.AspNetCore.Builder;

public static class SecurityHeadersApplicationBuilderExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseSecurityHeaders(new SecurityHeadersOptions());
    }

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app, SecurityHeadersOptions options)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(options);

        app.UseMiddleware<SecurityHeadersMiddleware>(options);

        return app;
    }

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app, Action<SecurityHeadersOptions> optionsAction)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(optionsAction);

        var options = new SecurityHeadersOptions();

        optionsAction.Invoke(options);

        return app.UseSecurityHeaders(options);
    }
}
