using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Title;
using OrchardCore.Modules;
using OrchardCore.Seo.Drivers;
using OrchardCore.Seo.Models;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.SeoMeta.Settings;
using OrchardCore.ContentTypes.Editors;

namespace OrchardCore.Seo
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDataMigration, Migrations>();

            services.AddContentPart<SeoMetaPart>()
                .UseDisplayDriver<SeoMetaPartDisplayDriver>()
                .AddHandler<SeoMetaPartHandler>();

            services.AddTransient<IContentDisplayDriver, SeoContentDriver>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, SeoMetaPartSettingsDisplayDriver>();

            // This must be last, and the module dependant on Contents so this runs after the part handlers.
            services.AddScoped<IContentHandler, SeoMetaSettingsHandler>();
        }
    }
}
