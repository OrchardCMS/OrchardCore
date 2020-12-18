using System;
using Ganss.XSS;
using OrchardCore.Infrastructure.Html;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HtmlSanitizerOptionsExtensions
    {
        /// <summary>
        /// Adds a configuration action to the html sanitizer.
        /// </summary>
        public static void ConfigureHtmlSanitizer(this IServiceCollection services, Action<HtmlSanitizer> action)
        {
            services.Configure<HtmlSanitizerOptions>(o =>
            {
                o.Configure.Add(action);
            });
        }
    }
}
