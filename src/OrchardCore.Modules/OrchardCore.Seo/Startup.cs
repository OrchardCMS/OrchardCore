using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.Seo.Drivers;
using OrchardCore.Seo.Indexes;
using OrchardCore.Seo.Models;
using OrchardCore.SeoMeta.Settings;
using YesSql.Indexes;

namespace OrchardCore.Seo
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddDataMigration<Migrations>();

            services.AddContentPart<SeoMetaPart>()
                .UseDisplayDriver<SeoMetaPartDisplayDriver>()
                .AddHandler<SeoMetaPartHandler>();

            services.AddScoped<IContentDisplayDriver, SeoContentDriver>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, SeoMetaPartSettingsDisplayDriver>();

            // This must be last, and the module dependant on Contents so this runs after the part handlers.
            services.AddScoped<IContentHandler, SeoMetaSettingsHandler>();
            services.AddScoped<IContentItemIndexHandler, SeoMetaPartContentIndexHandler>();
        }
    }
}
