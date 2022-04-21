using System;
using OrchardCore.Security.Options;

namespace Microsoft.AspNetCore.Builder
{
    public static class SecurityHeadersApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            ArgumentNullException.ThrowIfNull(app, nameof(app));

            return app.UseSecurityHeaders(new SecurityHeadersOptions());
        }

        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app, SecurityHeadersOptions options)
        {
            ArgumentNullException.ThrowIfNull(app, nameof(app));
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            app.UseContentTypeOptions();
            app.UseFrameOptions(options.FrameOptions);
            app.UsePermissionsPolicy(options.PermissionsPolicy);
            app.UseReferrerPolicy(options.ReferrerPolicy);
            app.UseStrictTransportSecurity(options.StrictTransportSecurity);

            return app;
        }

        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app, Action<SecurityHeadersOptionsBuilder> action)
        {
            ArgumentNullException.ThrowIfNull(app, nameof(app));
            ArgumentNullException.ThrowIfNull(action, nameof(action));

            var options = new SecurityHeadersOptions();
            var optionsBuilder = new SecurityHeadersOptionsBuilder(options);

            action.Invoke(optionsBuilder);

            return app.UseSecurityHeaders(options);
        }
    }
}
