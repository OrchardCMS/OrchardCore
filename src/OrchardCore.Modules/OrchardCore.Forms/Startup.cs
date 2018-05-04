using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.Forms.Drivers;
using OrchardCore.Forms.Models;
using OrchardCore.Modules;

namespace OrchardCore.Forms
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IContentPartDisplayDriver, FormPartDisplay>();
            services.AddSingleton<ContentPart, FormPart>();
            services.AddScoped<IDataMigration, Migrations>();
        }
    }
}
