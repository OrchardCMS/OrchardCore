using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Body.Drivers;
using Orchard.Body.Model;
using Orchard.Body.Settings;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentTypes.Editors;
using Orchard.Data.Migration;
using Orchard.Tokens;

namespace Orchard.Body
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Body Part
            services.AddScoped<IContentPartDisplayDriver, BodyPartDisplay>();
            services.AddSingleton<ContentPart, BodyPart>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, BodyPartSettingsDisplayDriver>();
            services.AddScoped<IDataMigration, Migrations>();

            services.AddNullTokenizer();
        }
    }
}
