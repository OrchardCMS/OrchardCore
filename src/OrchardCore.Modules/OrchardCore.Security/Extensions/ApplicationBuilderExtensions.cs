using System;
using OrchardCore.Security;
using OrchardCore.Security.Middlewares;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
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

            app.UseMiddleware<ContentTypeOptionsMiddleware>(settings.ContentTypeOptions);
            app.UseMiddleware<FrameOptionsMiddleware>(settings.FrameOptions);
            app.UseMiddleware<PermissionsPolicyMiddleware>(settings.PermissionsPolicy);
            app.UseMiddleware<ReferrerPolicyMiddleware>(settings.ReferrerPolicy);

            return app;
        }
    }
}
