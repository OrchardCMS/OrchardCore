using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Orchard.DependencyInjection;
using Orchard.Roles.Services;
using Orchard.Security;

namespace Orchard.Roles
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddScoped<RoleManager<Role>, RoleManager<Role>>();
            serviceCollection.TryAddScoped<IRoleStore<Role>, RoleStore>();
            serviceCollection.TryAddScoped<IRoleManager, RoleManager>();
        }
    }
}
