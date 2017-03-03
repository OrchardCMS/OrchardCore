using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Body.Drivers;
using Orchard.Body.Handlers;
using Orchard.Body.Indexing;
using Orchard.Body.Model;
using Orchard.Body.Settings;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentTypes.Editors;
using Orchard.Data.Migration;
using Orchard.Indexing;
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
            services.AddScoped<IContentPartIndexHandler, BodyPartIndexHandler>();
            services.AddScoped<IContentPartHandler, BodyPartHandler>();

            services.AddNullTokenizer();
        }
    }
}
