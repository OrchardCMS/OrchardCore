using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.Markdown.Fields;
using OrchardCore.Markdown.Models;
using OrchardCore.Modules;

namespace OrchardCore.Markdown.GraphQL
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddObjectGraphType<MarkdownBodyPart, MarkdownBodyQueryObjectType>();
            services.AddObjectGraphType<MarkdownField, MarkdownFieldQueryObjectType>();
        }
    }
}
