using OrchardCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using GraphQL.Types;
using OrchardCore.Alias.Models;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.ContentManagement;

namespace OrchardCore.Alias.GraphQL
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<InputObjectGraphType<AliasPart>, AliasInputObjectType>();
            services.AddScoped<IInputObjectGraphType, AliasInputObjectType>();

            services.AddScoped<ObjectGraphType<AliasPart>, AliasQueryObjectType>();
            services.AddScoped<IObjectGraphType, AliasQueryObjectType>();

            services.AddScoped<IGraphQLFilter<ContentItem>, AliasGraphQLFilter>();
        }
    }
}
