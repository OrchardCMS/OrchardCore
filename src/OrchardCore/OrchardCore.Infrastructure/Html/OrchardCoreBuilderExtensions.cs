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
                services.AddScoped<IHtmlSanitizerService, HtmlSanitizerService>();
            });

            return builder;
        }
    }
}
