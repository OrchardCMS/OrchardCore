using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;
using OrchardCore.Widgets.Drivers;
using OrchardCore.Widgets.Models;
using OrchardCore.Widgets.Services;
using OrchardCore.Widgets.Settings;

namespace OrchardCore.Widgets;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        // Add Widget Card Shapes
        services.AddShapeTableProvider<ContentCardShapes>();
        // Widgets List Part
        services.AddContentPart<WidgetsListPart>()
            .UseDisplayDriver<WidgetsListPartDisplayDriver>();

        services.AddScoped<IStereotypesProvider, WidgetStereotypesProvider>();

        services.AddScoped<IContentTypePartDefinitionDisplayDriver, WidgetsListPartSettingsDisplayDriver>();
        services.AddContentPart<WidgetMetadata>();
        services.AddDataMigration<Migrations>();
        services.AddTransient<IConfigureOptions<ResourceManagementOptions>, ResourceManagementOptionsConfiguration>();
    }
}
