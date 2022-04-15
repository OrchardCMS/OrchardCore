using System;
using OrchardCore.Security;
using OrchardCore.Security.Middlewares;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseReferrerPolicy(this IApplicationBuilder app, string policyOption)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (String.IsNullOrEmpty(policyOption))
            {
                throw new ArgumentException($"'{nameof(policyOption)}' cannot be null or empty.", nameof(policyOption));
            }

            return app.UseMiddleware<ReferrerPolicyMiddleware>(policyOption);
        }

        public static IApplicationBuilder UseFrameOptions(this IApplicationBuilder app, string option)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (String.IsNullOrEmpty(option))
            {
                throw new ArgumentException($"'{nameof(option)}' cannot be null or empty.", nameof(option));
            }

            return app.UseMiddleware<FrameOptionsMiddleware>(option);
        }

        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseSecurityHeaders(config => { });
        }

        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app, Action<SecurityHeadersBuilder> configureSecurityHeaders)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (configureSecurityHeaders is null)
            {
                throw new ArgumentNullException(nameof(configureSecurityHeaders));
            }

            var settings = new SecuritySettings();
            var builder = new SecurityHeadersBuilder(settings);

            configureSecurityHeaders.Invoke(builder);

            settings = builder.Build();

            app.UseReferrerPolicy(settings.ReferrerPolicy);
            app.UseFrameOptions(settings.FrameOptions);

            return app;
        }
    }
}
