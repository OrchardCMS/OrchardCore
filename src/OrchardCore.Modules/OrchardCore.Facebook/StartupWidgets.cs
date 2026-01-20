using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Facebook.Widgets;
using OrchardCore.Facebook.Widgets.Drivers;
using OrchardCore.Facebook.Widgets.Handlers;
using OrchardCore.Facebook.Widgets.Models;
using OrchardCore.Facebook.Widgets.Services;
using OrchardCore.Facebook.Widgets.Settings;
using OrchardCore.Modules;

namespace OrchardCore.Facebook
{
    [Feature(FacebookConstants.Features.Widgets)]
    public class StartupWidgets : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddDataMigration<WidgetMigrations>();
            services.AddScoped<IShapeTableProvider, LiquidShapes>();

            services.AddContentPart<FacebookPluginPart>()
                .UseDisplayDriver<FacebookPluginPartDisplayDriver>()
                .AddHandler<FacebookPluginPartHandler>();

            services.AddScoped<IContentTypePartDefinitionDisplayDriver, FacebookPluginPartSettingsDisplayDriver>();
        }
    }
}
