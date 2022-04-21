using System;
using Microsoft.Extensions.Options;
using OrchardCore.Security.Options;
using OrchardCore.Security.Middlewares;

namespace Microsoft.AspNetCore.Builder
{
    public static class FrameOptionsApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseFrameOptions(this IApplicationBuilder app)
        {
            ArgumentNullException.ThrowIfNull(app, nameof(app));

            var options = new FrameOptionsOptions { Value = FrameOptionsValue.SameOrigin };

            return app.UseFrameOptions(options);
        }

        public static IApplicationBuilder UseFrameOptions(this IApplicationBuilder app, FrameOptionsOptions options)
        {
            ArgumentNullException.ThrowIfNull(app, nameof(app));
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            return app.UseMiddleware<FrameOptionsMiddleware>(Options.Create(options));
        }

        public static IApplicationBuilder UseFrameOptions(this IApplicationBuilder app, Action<FrameOptionsOptionsBuilder> action)
        {
            ArgumentNullException.ThrowIfNull(app, nameof(app));
            ArgumentNullException.ThrowIfNull(action, nameof(action));

            var options = new FrameOptionsOptions();
            var builder = new FrameOptionsOptionsBuilder(options);

            action(builder);

            return app.UseFrameOptions(options);
        }
    }
}
