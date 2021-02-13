using OrchardCore.MetaTags.Drivers;
using OrchardCore.MetaTags.Models;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;

namespace OrchardCore.MetaTags
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddContentPart<MetaTagsPart>()
                .UseDisplayDriver<MetaTagsPartDisplayDriver>();
            services.AddScoped<IDataMigration, Migrations>();
        }
    }
}
