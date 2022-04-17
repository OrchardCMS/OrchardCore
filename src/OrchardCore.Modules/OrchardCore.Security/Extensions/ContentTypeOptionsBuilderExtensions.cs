using System;
using Microsoft.Extensions.Options;
using OrchardCore.Security;
using OrchardCore.Security.Middlewares;

namespace Microsoft.AspNetCore.Builder
{
    public static class ContentTypeOptionsBuilderExtensions
    {
        public static IApplicationBuilder UseContentTypeOptions(this IApplicationBuilder app)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var options = new ContentTypeOptionsOptions();

            return app.UseContentTypeOptions(options);
        }

        public static IApplicationBuilder UseContentTypeOptions(this IApplicationBuilder app, ContentTypeOptionsOptions options)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return app.UseMiddleware<ContentTypeOptionsMiddleware>(Options.Create(options));
        }
    }
}
