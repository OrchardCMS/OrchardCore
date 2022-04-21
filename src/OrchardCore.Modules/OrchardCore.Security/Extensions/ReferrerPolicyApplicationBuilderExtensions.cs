using System;
using Microsoft.Extensions.Options;
using OrchardCore.Security;
using OrchardCore.Security.Middlewares;

namespace Microsoft.AspNetCore.Builder
{
    public static class ReferrerPolicyApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseReferrerPolicy(this IApplicationBuilder app)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var options = new ReferrerPolicyOptions { Value = ReferrerPolicyValue.NoReferrer };

            return app.UseReferrerPolicy(options);
        }

        public static IApplicationBuilder UseReferrerPolicy(this IApplicationBuilder app, ReferrerPolicyOptions options)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return app.UseMiddleware<ReferrerPolicyMiddleware>(Options.Create(options));
        }

        public static IApplicationBuilder UseReferrerPolicy(this IApplicationBuilder app, Action<ReferrerPolicyOptionsBuilder> actions)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (actions is null)
            {
                throw new ArgumentNullException(nameof(actions));
            }

            var options = new ReferrerPolicyOptions();
            var builder = new ReferrerPolicyOptionsBuilder(options);

            actions(builder);

            return app.UseReferrerPolicy(options);
        }
    }
}
