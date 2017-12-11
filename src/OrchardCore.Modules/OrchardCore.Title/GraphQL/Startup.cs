using OrchardCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using GraphQL.Types;
using OrchardCore.Title.Model;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.ContentManagement;

namespace OrchardCore.Title.GraphQL
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<InputObjectGraphType<TitlePart>, TitleInputObjectType>();
            services.AddScoped<IInputObjectGraphType, TitleInputObjectType>();

            services.AddScoped<ObjectGraphType<TitlePart>, TitleQueryObjectType>();
            services.AddScoped<IObjectGraphType, TitleQueryObjectType>();

            services.AddScoped<IGraphQLFilter<ContentItem>, TitleGraphQLFilter>();
        }
    }
}
