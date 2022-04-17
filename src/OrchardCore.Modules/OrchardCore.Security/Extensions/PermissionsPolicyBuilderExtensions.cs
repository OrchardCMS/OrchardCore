using System;
using Microsoft.Extensions.Options;
using OrchardCore.Security;
using OrchardCore.Security.Middlewares;

namespace Microsoft.AspNetCore.Builder
{
    public static class PermissionsPolicyBuilderExtensions
    {
        public static IApplicationBuilder UsePermissionsPolicy(this IApplicationBuilder app)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var options = new PermissionsPolicyOptions();

            return app.UsePermissionsPolicy(options);
        }

        public static IApplicationBuilder UsePermissionsPolicy(this IApplicationBuilder app, PermissionsPolicyOptions options)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return app.UseMiddleware<PermissionsPolicyMiddleware>(Options.Create(options));
        }
    }
}
