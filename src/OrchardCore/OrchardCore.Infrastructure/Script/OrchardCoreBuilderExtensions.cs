using OrchardCore.Infrastructure.Script;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides an extension method for <see cref="OrchardCoreBuilder"/>.
    /// </summary>
    public static partial class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds e-mail address validator service.
        /// </summary>
        /// <param name="builder">The <see cref="OrchardCoreBuilder"/>.</param>
        public static OrchardCoreBuilder AddScriptProtection(this OrchardCoreBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped<IHtmlScriptSanitizer, HtmlScriptSanitizer>();
            });

            return builder;
        }
    }
}
