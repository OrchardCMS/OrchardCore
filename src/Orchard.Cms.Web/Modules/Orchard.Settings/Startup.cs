using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Recipes;
using Orchard.Settings.Recipes;
using Orchard.Settings.Services;

namespace Orchard.Settings
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<SetupEventHandler>();
            services.AddScoped<ISetupEventHandler>(sp => sp.GetRequiredService<SetupEventHandler>());

            services.AddRecipeExecutionStep<SettingsStep>();
            services.AddScoped<ISiteService, SiteService>();
        }
    }
}
