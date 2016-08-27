using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Orchard.Environment.Shell;
using Orchard.Roles.Services;
using Orchard.Security;

namespace Orchard.Roles
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.TryAddScoped<RoleManager<Role>, RoleManager<Role>>();
            services.TryAddScoped<IRoleStore<Role>, RoleStore>();
            services.TryAddScoped<IRoleManager, RoleManager>();

            services.AddScoped<RoleUpdater>();
            services.AddScoped<IFeatureEventHandler>(sp => sp.GetRequiredService<RoleUpdater>());

        }
    }
}
