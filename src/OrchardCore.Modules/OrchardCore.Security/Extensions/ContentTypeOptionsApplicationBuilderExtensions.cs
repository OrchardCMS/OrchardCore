using System;
using Microsoft.Extensions.Options;
using OrchardCore.Security.Options;
using OrchardCore.Security.Middlewares;

namespace Microsoft.AspNetCore.Builder
{
    public static class ContentTypeOptionsApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseContentTypeOptions(this IApplicationBuilder app)
        {
            ArgumentNullException.ThrowIfNull(app, nameof(app));

            var options = new ContentTypeOptionsOptions();

            return app.UseContentTypeOptions(options);
        }

        public static IApplicationBuilder UseContentTypeOptions(this IApplicationBuilder app, ContentTypeOptionsOptions options)
        {
            ArgumentNullException.ThrowIfNull(app, nameof(app));
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            return app.UseMiddleware<ContentTypeOptionsMiddleware>(Options.Create(options));
        }
    }
}
