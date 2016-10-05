using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Body.Drivers;
using Orchard.Body.Settings;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentTypes.Editors;
using Orchard.Data.Migration;

namespace Orchard.Body
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Body Part
            services.AddScoped<IContentPartDisplayDriver, BodyPartDisplay>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, BodyPartSettingsDisplayDriver>();
            services.AddScoped<IContentPartDriver, BodyPartDriver>();
            services.AddScoped<IContentHandler, BodyPartDriver>();
            services.AddScoped<IDataMigration, Migrations>();
        }
    }
}
