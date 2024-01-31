using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.Autoroute.Core.Indexes;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement.GraphQL;
using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.Modules;

namespace OrchardCore.Autoroute.GraphQL
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddInputObjectGraphType<AutoroutePart, AutorouteInputObjectType>();
            services.AddObjectGraphType<AutoroutePart, AutorouteQueryObjectType>();
            services.AddTransient<IIndexAliasProvider, AutoroutePartIndexAliasProvider>();
            services.AddWhereInputIndexPropertyProvider<AutoroutePartIndex>();
        }
    }
}
