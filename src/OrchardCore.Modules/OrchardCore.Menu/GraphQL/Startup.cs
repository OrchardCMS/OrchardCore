using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.Menu.Models;
using OrchardCore.Modules;

namespace OrchardCore.Menu.GraphQL
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddObjectGraphType<MenuItemsListPart, MenuItemsListQueryObjectType>();
            services.AddObjectGraphType<LinkMenuItemPart, LinkMenuItemQueryObjectType>();
            services.AddObjectGraphType<HtmlMenuItemPart, HtmlMenuItemQueryObjectType>();
            services.AddScoped<IContentTypeBuilder, MenuItemContentTypeBuilder>();
        }
    }
}
