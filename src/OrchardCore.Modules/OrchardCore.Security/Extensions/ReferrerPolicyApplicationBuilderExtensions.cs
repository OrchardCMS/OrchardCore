using System;
using Microsoft.Extensions.Options;
using OrchardCore.Security.Options;
using OrchardCore.Security.Middlewares;

namespace Microsoft.AspNetCore.Builder
{
    public static class ReferrerPolicyApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseReferrerPolicy(this IApplicationBuilder app)
        {
            ArgumentNullException.ThrowIfNull(app, nameof(app));

            var options = new ReferrerPolicyOptions { Value = ReferrerPolicyValue.NoReferrer };

            return app.UseReferrerPolicy(options);
        }

        public static IApplicationBuilder UseReferrerPolicy(this IApplicationBuilder app, ReferrerPolicyOptions options)
        {
            ArgumentNullException.ThrowIfNull(app, nameof(app));
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            return app.UseMiddleware<ReferrerPolicyMiddleware>(Options.Create(options));
        }

        public static IApplicationBuilder UseReferrerPolicy(this IApplicationBuilder app, Action<ReferrerPolicyOptionsBuilder> action)
        {
            ArgumentNullException.ThrowIfNull(app, nameof(app));
            ArgumentNullException.ThrowIfNull(action, nameof(action));

            var options = new ReferrerPolicyOptions();
            var builder = new ReferrerPolicyOptionsBuilder(options);

            action(builder);

            return app.UseReferrerPolicy(options);
        }
    }
}
