using System;
using OrchardCore.Security;

namespace Microsoft.AspNetCore.Builder
{
    public static class SecurityHeadersApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseSecurityHeaders(new SecurityHeadersOptions());
        }

        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app, SecurityHeadersOptions options)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            app.UseContentTypeOptions();
            app.UseFrameOptions(options.FrameOptions);
            app.UsePermissionsPolicy(options.PermissionsPolicy);
            app.UseReferrerPolicy(options.ReferrerPolicy);
            app.UseStrictTransportSecurity(options.StrictTransportSecurity);

            return app;
        }

        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app, Action<SecurityHeadersOptionsBuilder> action)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var options = new SecurityHeadersOptions();
            var optionsBuilder = new SecurityHeadersOptionsBuilder(options);

            action.Invoke(optionsBuilder);

            return app.UseSecurityHeaders(options);
        }
    }
}
