using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Handlers;
using Orchard.Data.Migration;
using Orchard.Lists.Drivers;
using Orchard.Menu.Handlers;
using Orchard.Menu.Models;

namespace Orchard.Menu
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDataMigration, Migrations>();

            // MenuPart
            services.AddScoped<IContentHandler, MenuContentHandler>();
            services.AddScoped<IContentPartDisplayDriver, MenuPartDisplayDriver>();
            services.AddSingleton<ContentPart, MenuPart>();

            // LinkMenuItemPart
            services.AddScoped<IContentPartDisplayDriver, LinkMenuItemPartDisplayDriver>();
            services.AddSingleton<ContentPart, LinkMenuItemPart>();
        }
    }
}
