using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Orchard.DisplayManagement.Fluid;
using Orchard.Environment.Navigation;
using Orchard.Security.Permissions;
using Orchard.Templates.Services;

namespace Orchard.Templates
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<FluidViewOptions>, TemplateFluidViewOptionsSetup>());

            services.AddSingleton<ITemplateFileProvider, TemplateFileProvider>();

            services.AddScoped<TemplatesManager>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();
        }
    }
}
