using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.Flows.Models;
using OrchardCore.Modules;

namespace OrchardCore.Flows.GraphQL
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddObjectGraphType<BagPart, BagPartQueryObjectType>();
            services.AddObjectGraphType<FlowPart, FlowPartQueryObjectType>();
            services.AddObjectGraphType<FlowMetadata, FlowMetadataQueryObjectType>();

            services.AddScoped<IContentTypeBuilder, FlowMetadataContentTypeBuilder>();
        }
    }
}
