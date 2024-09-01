using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Seo.Drivers;
using OrchardCore.Seo.Indexes;
using OrchardCore.Seo.Models;
using OrchardCore.Seo.Services;
using OrchardCore.SeoMeta.Settings;

namespace OrchardCore.Seo;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataMigration<Migrations>();

        services.AddContentPart<SeoMetaPart>()
            .UseDisplayDriver<SeoMetaPartDisplayDriver>()
            .AddHandler<SeoMetaPartHandler>();

        services.AddScoped<IContentDisplayDriver, SeoContentDriver>();
        services.AddScoped<IContentTypePartDefinitionDisplayDriver, SeoMetaPartSettingsDisplayDriver>();

        // This must be last, and the module dependent on Contents so this runs after the part handlers.
        services.AddScoped<IContentHandler, SeoMetaSettingsHandler>();
        services.AddScoped<IContentItemIndexHandler, SeoMetaPartContentIndexHandler>();

        services.AddPermissionProvider<SeoPermissionProvider>();
        services.AddSiteDisplayDriver<RobotsSettingsDisplayDriver>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddTransient<IRobotsProvider, SiteSettingsRobotsProvider>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        app.UseMiddleware<RobotsMiddleware>();
    }
}
