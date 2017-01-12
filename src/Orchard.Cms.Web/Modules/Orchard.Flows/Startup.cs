using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Handlers;
using Orchard.Data.Migration;
using Orchard.Flows.Drivers;
using Orchard.Flows.Handlers;
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
            services.AddScoped<IContentPartHandler, FlowPartHandler>();

            services.AddSingleton<ContentPart, FlowMetadata>();
            services.AddScoped<IDataMigration, Migrations>();
        }
    }
}
