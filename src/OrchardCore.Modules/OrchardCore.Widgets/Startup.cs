using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Modules;
using OrchardCore.Widgets.Drivers;
using OrchardCore.Widgets.Models;
using OrchardCore.Widgets.Settings;

namespace OrchardCore.Widgets
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Add Widget Card Shapes
            services.AddScoped<IShapeTableProvider, ContentCardShapes>();
            // Widgets List Part
            services.AddContentPart<WidgetsListPart>()
                .UseDisplayDriver<WidgetsListPartDisplayDriver>();

            services.AddScoped<IContentTypePartDefinitionDisplayDriver, WidgetsListPartSettingsDisplayDriver>();
            services.AddContentPart<WidgetMetadata>();
            services.AddDataMigration<Migrations>();
        }
    }
}
