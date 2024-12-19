using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement;
using OrchardCore.Menu.Drivers;
using OrchardCore.Menu.Handlers;
using OrchardCore.Menu.Models;
using OrchardCore.Menu.Services;
using OrchardCore.Menu.Settings;
using OrchardCore.Menu.TagHelpers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Menu;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataMigration<Migrations>();
        services.AddShapeTableProvider<MenuShapes>();
        services.AddPermissionProvider<Permissions>();
        services.AddNavigationProvider<AdminMenu>();

        services.AddScoped<IStereotypesProvider, MenuItemStereotypesProvider>();

        // MenuPart
        services.AddScoped<IContentHandler, MenuContentHandler>();
        services.AddContentPart<MenuPart>()
            .UseDisplayDriver<MenuPartDisplayDriver>();

        services.AddContentPart<MenuItemsListPart>();

        // LinkMenuItemPart
        services.AddContentPart<LinkMenuItemPart>()
            .UseDisplayDriver<LinkMenuItemPartDisplayDriver>();

        // ContentMenuItemPart
        services.AddContentPart<ContentMenuItemPart>()
            .UseDisplayDriver<ContentMenuItemPartDisplayDriver>();

        // HtmlMenuItemPart
        services.AddContentPart<HtmlMenuItemPart>()
            .UseDisplayDriver<HtmlMenuItemPartDisplayDriver>();
        services.AddScoped<IContentTypePartDefinitionDisplayDriver, HtmlMenuItemPartSettingsDisplayDriver>();

        services.AddTagHelpers<MenuTagHelper>();
    }
}
