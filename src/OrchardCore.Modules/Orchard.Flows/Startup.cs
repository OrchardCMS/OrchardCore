using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.Data.Migration;
using Orchard.Flows.Drivers;
using Orchard.Flows.Models;

namespace Orchard.Flows
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Flow Part
            services.AddScoped<IContentPartDisplayDriver, FlowPartDisplay>();
            services.AddSingleton<ContentPart, FlowPart>();
            services.AddScoped<IContentDisplayDriver, FlowMetadataDisplay>();

            services.AddSingleton<ContentPart, FlowMetadata>();
            services.AddScoped<IDataMigration, Migrations>();
        }
    }
}
