using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.ContentManagement.GraphQL;
using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.Lists.Indexes;
using OrchardCore.Lists.Models;
using OrchardCore.Modules;

namespace OrchardCore.Lists.GraphQL
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddInputObjectGraphType<ContainedPart, ContainedInputObjectType>();
            services.AddObjectGraphType<ContainedPart, ContainedQueryObjectType>();
            services.AddObjectGraphType<ListPart, ListQueryObjectType>();
            services.AddTransient<IIndexAliasProvider, ContainedPartIndexAliasProvider>();
            services.AddWhereInputIndexPropertyProvider<ContainedPartIndex>();
        }
    }
}
