using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Tenants.Workflows.Activities;
using OrchardCore.Tenants.Workflows.Drivers;
using OrchardCore.Modules;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tenants.Workflows
{
    [RequireFeatures("OrchardCore.Workflows")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddActivity<DisableTenantTask, DisableTenantTaskDisplay>();
            services.AddActivity<EnableTenantTask, EnableTenantTaskDisplay>();
            services.AddActivity<CreateTenantTask, CreateTenantTaskDisplay>();
            services.AddActivity<SetupTenantTask, SetupTenantTaskDisplay>();
        }
    }
}
