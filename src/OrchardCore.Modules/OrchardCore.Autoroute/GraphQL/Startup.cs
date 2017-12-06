using OrchardCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using GraphQL.Types;
using OrchardCore.Autoroute.Model;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.ContentManagement;

namespace OrchardCore.Autoroute.GraphQL
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<InputObjectGraphType<AutoroutePart>, AutorouteInputObjectType>();
            services.AddScoped<IInputObjectGraphType, AutorouteInputObjectType>();

            services.AddScoped<ObjectGraphType<AutoroutePart>, AutorouteQueryObjectType>();
            services.AddScoped<IObjectGraphType, AutorouteQueryObjectType>();

            services.AddScoped<IGraphQLFilter<ContentItem>, AutorouteGraphQLFilter>();
        }
    }
}
