using OrchardCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Body.Drivers;
using OrchardCore.Body.Handlers;
using OrchardCore.Body.Indexing;
using OrchardCore.Body.Model;
using OrchardCore.Body.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Indexing;

namespace OrchardCore.Body
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
        }
    }
}
