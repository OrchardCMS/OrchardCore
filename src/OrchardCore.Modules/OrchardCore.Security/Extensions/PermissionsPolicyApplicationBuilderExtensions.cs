using System;
using Microsoft.Extensions.Options;
using OrchardCore.Security.Options;
using OrchardCore.Security.Middlewares;

namespace Microsoft.AspNetCore.Builder
{
    public static class PermissionsPolicyApplicationBuilderExtensions
    {
        public static IApplicationBuilder UsePermissionsPolicy(this IApplicationBuilder app)
        {
            ArgumentNullException.ThrowIfNull(app, nameof(app));

            var options = new PermissionsPolicyOptions();

            return app.UsePermissionsPolicy(options);
        }

        public static IApplicationBuilder UsePermissionsPolicy(this IApplicationBuilder app, PermissionsPolicyOptions options)
        {
            ArgumentNullException.ThrowIfNull(app, nameof(app));
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            return app.UseMiddleware<PermissionsPolicyMiddleware>(Options.Create(options));
        }

        public static IApplicationBuilder UsePermissionsPolicy(this IApplicationBuilder app, Action<PermissionsPolicyOptionsBuilder> action)
        {
            ArgumentNullException.ThrowIfNull(app, nameof(app));
            ArgumentNullException.ThrowIfNull(action, nameof(action));

            var options = new PermissionsPolicyOptions();
            var builder = new PermissionsPolicyOptionsBuilder(options);

            action(builder);

            return app.UsePermissionsPolicy(options);
        }
    }
}
