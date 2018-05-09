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
            services.AddScoped<IContentPartDisplayDriver, ButtonPartDisplay>();
            services.AddScoped<IContentPartDisplayDriver, InputPartDisplay>();
            services.AddScoped<IContentPartDisplayDriver, TextAreaPartDisplay>();

            services.AddSingleton<ContentPart, FormPart>();
            services.AddSingleton<ContentPart, ButtonPart>();
            services.AddSingleton<ContentPart, InputPart>();
            services.AddSingleton<ContentPart, TextAreaPart>();

            services.AddScoped<IDataMigration, Migrations>();
        }
    }
}
