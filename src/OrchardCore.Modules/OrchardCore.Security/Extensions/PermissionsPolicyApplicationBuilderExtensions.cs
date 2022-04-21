using System;
using Microsoft.Extensions.Options;
using OrchardCore.Security;
using OrchardCore.Security.Middlewares;

namespace Microsoft.AspNetCore.Builder
{
    public static class PermissionsPolicyApplicationBuilderExtensions
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

        public static IApplicationBuilder UsePermissionsPolicy(this IApplicationBuilder app, Action<PermissionsPolicyOptionsBuilder> actions)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (actions is null)
            {
                throw new ArgumentNullException(nameof(actions));
            }

            var options = new PermissionsPolicyOptions();
            var builder = new PermissionsPolicyOptionsBuilder(options);

            actions(builder);

            return app.UsePermissionsPolicy(options);
        }
    }
}
