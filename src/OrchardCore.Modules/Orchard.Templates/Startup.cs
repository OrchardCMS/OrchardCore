using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy;
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
            services.AddScoped<ITemplateFileProvider, TemplateFileProvider>();

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<FluidViewOptions>,
                TemplateOptionsSetup<FluidViewOptions>>());

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<RazorViewEngineOptions>,
                TemplateOptionsSetup<RazorViewEngineOptions>>());

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<ShapeTemplateOptions>,
                TemplateOptionsSetup<ShapeTemplateOptions>>());

            services.AddScoped<TemplatesManager>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();
        }
    }
}
