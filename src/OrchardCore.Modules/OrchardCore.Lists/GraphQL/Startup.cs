using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.ContentManagement;
using OrchardCore.Lists.Models;
using OrchardCore.Modules;

namespace OrchardCore.Lists.GraphQL
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<InputObjectGraphType<ContainedPart>, ContainedInputObjectType>();
            services.AddScoped<IInputObjectGraphType, ContainedInputObjectType>();

            services.AddScoped<ObjectGraphType<ContainedPart>, ContainedQueryObjectType>();
            services.AddScoped<IObjectGraphType, ContainedQueryObjectType>();

            services.AddScoped<IGraphQLFilter<ContentItem>, ContainedGraphQLFilter>();
        }
    }
}
