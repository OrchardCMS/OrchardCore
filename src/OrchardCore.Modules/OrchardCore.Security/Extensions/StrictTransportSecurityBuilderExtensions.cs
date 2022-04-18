using System;
using Microsoft.Extensions.Options;
using OrchardCore.Security;
using OrchardCore.Security.Middlewares;

namespace Microsoft.AspNetCore.Builder
{
    public static class StrictTransportSecurityBuilderExtensions
    {
        public static IApplicationBuilder UseStrictTransportSecurity(this IApplicationBuilder app)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseStrictTransportSecurity(new StrictTransportSecurityOptions());
        }

        public static IApplicationBuilder UseStrictTransportSecurity(this IApplicationBuilder app, StrictTransportSecurityOptions options)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return app.UseMiddleware<StrictTransportSecurityMiddleware>(Options.Create(options));
        }
    }
}
