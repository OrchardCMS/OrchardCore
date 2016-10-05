using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Data.Migration;
using Orchard.Title.Drivers;

namespace Orchard.Title
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Title Part
            services.AddScoped<IContentPartDisplayDriver, TitlePartDisplay>();
            services.AddScoped<IContentPartDriver, TitlePartDriver>();
            services.AddScoped<IContentHandler, TitlePartDriver>();

            services.AddScoped<IDataMigration, Migrations>();
        }
    }
}
