using System;
using Microsoft.Extensions.Options;
using OrchardCore.Security;
using OrchardCore.Security.Middlewares;

namespace Microsoft.AspNetCore.Builder
{
    public static class FrameOptionsApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseFrameOptions(this IApplicationBuilder app)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var options = new FrameOptionsOptions { Value = FrameOptionsValue.SameOrigin };

            return app.UseFrameOptions(options);
        }

        public static IApplicationBuilder UseFrameOptions(this IApplicationBuilder app, FrameOptionsOptions options)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return app.UseMiddleware<FrameOptionsMiddleware>(Options.Create(options));
        }

        public static IApplicationBuilder UseFrameOptions(this IApplicationBuilder app, Action<FrameOptionsOptionsBuilder> actions)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (actions is null)
            {
                throw new ArgumentNullException(nameof(actions));
            }

            var options = new FrameOptionsOptions();
            var builder = new FrameOptionsOptionsBuilder(options);

            actions(builder);

            return app.UseFrameOptions(options);
        }
    }
}
