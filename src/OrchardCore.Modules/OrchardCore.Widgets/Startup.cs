using OrchardCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Widgets.Drivers;
using OrchardCore.Widgets.Models;
using OrchardCore.Widgets.Settings;

namespace OrchardCore.Widgets
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Widgets List Part
            services.AddScoped<IContentPartDisplayDriver, WidgetsListPartDisplay>();
            services.AddContentPart<WidgetsListPart>();

            services.AddScoped<IContentTypePartDefinitionDisplayDriver, WidgetsListPartSettingsDisplayDriver>();
            services.AddContentPart<WidgetMetadata>();
            services.AddScoped<IDataMigration, Migrations>();
        }
    }
}
