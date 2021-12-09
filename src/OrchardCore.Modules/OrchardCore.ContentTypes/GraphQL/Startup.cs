using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.ContentTypes.GraphQL.Drivers;
using OrchardCore.Modules;

namespace OrchardCore.ContentTypes.GraphQL
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, GraphQLContentTypePartSettingsDriver>();
        }
    }
}
