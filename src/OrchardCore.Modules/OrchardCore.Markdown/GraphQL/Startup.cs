using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.Markdown.Model;
using OrchardCore.Modules;

namespace OrchardCore.Markdown.GraphQL
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddGraphQLInputType<MarkdownBodyPart, MarkdownBodyInputObjectType>();
            services.AddGraphQLQueryType<MarkdownBodyPart, MarkdownBodyQueryObjectType>();
        }
    }
}
