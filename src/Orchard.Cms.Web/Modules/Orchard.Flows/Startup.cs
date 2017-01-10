using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.Data.Migration;
using Orchard.Flows.Drivers;
using Orchard.Flows.Model;

namespace Orchard.Flows
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Flow Part
            services.AddScoped<IContentPartDisplayDriver, FlowPartDisplay>();
            services.AddSingleton<ContentPart, FlowPart>();
            services.AddScoped<IDataMigration, Migrations>();
        }
    }
}
