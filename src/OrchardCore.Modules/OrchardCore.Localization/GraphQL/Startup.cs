using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Modules;

namespace OrchardCore.Localization.GraphQL
{
    /// <summary>
    /// Represents the localization module entry point for Graph QL.
    /// </summary>
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        /// <inheritdocs />
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ISchemaBuilder, SiteCulturesQuery>();
            services.AddTransient<CultureQueryObjectType>();
        }
    }
}
