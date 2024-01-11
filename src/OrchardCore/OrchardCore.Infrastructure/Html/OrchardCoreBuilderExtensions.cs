using OrchardCore.Infrastructure.Html;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds html script sanitization services.
        /// </summary>
        /// <param name="builder">The <see cref="OrchardCoreBuilder"/>.</param>
        public static OrchardCoreBuilder AddHtmlSanitizer(this OrchardCoreBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddOptions<HtmlSanitizerOptions>();

                services.ConfigureHtmlSanitizer((sanitizer) =>
                {
                    sanitizer.AllowedAttributes.Add("class");
                    sanitizer.AllowedTags.Remove("form");
                });

                services.AddSingleton<IHtmlSanitizerService, HtmlSanitizerService>();
            });

            return builder;
        }
    }
}
