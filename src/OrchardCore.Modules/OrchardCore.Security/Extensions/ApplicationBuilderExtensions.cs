using System;
using System.Linq;
using OrchardCore.Security;

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

            app.UseContentTypeOptions();
            app.UseFrameOptions(new FrameOptionsOptions
            {
                Value = new FrameOptionsValue(settings.FrameOptions)
            });

            var permissionsPolicyOptions = new PermissionsPolicyOptions();
            if (settings.PermissionsPolicy.Count > 0)
            {
                permissionsPolicyOptions.Values = settings.PermissionsPolicy
                    .Select(p => new PermissionsPolicyValue(p))
                    .ToList();
                permissionsPolicyOptions.Origin = new PermissionsPolicyOriginValue(settings.PermissionsPolicyOrigin);
            }

            app.UsePermissionsPolicy(permissionsPolicyOptions);

            app.UseReferrerPolicy(new ReferrerPolicyOptions
            {
                Value = new ReferrerPolicyValue(settings.ReferrerPolicy)
            });

            app.UseStrictTransportSecurity(settings.StrictTransportSecurity);

            return app;
        }
    }
}
