using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentTypes.Editors;
using Orchard.Data.Migration;
using Orchard.Widgets.Drivers;
using Orchard.Widgets.Models;
using Orchard.Widgets.Settings;

namespace Orchard.Widgets
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Widgets List Part
            services.AddScoped<IContentPartDisplayDriver, WidgetsListPartDisplay>();
            services.AddSingleton<ContentPart, WidgetsListPart>();

            services.AddScoped<IContentTypePartDefinitionDisplayDriver, WidgetsListPartSettingsDisplayDriver>();
            services.AddSingleton<ContentPart, WidgetMetadata>();
            services.AddScoped<IDataMigration, Migrations>();
        }
    }
}
