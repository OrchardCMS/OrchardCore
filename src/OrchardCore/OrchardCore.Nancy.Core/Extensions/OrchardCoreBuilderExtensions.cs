using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Nancy;
using Nancy.Owin;
using OrchardCore.Modules;
using OrchardCore.Modules.Extensions;
using OrchardCore.Nancy.AssemblyCatalogs;

namespace OrchardCore.Nancy
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level Nancy services and configuration.
        /// </summary>
        /// <param name="services"></param>
        public static OrchardCoreBuilder AddNancy(this OrchardCoreBuilder builder)
        {
            return builder.Startup.ConfigureServices((collection, sp) =>
            {
                collection.AddRouting();
            })
            .Configure((app, routes, sp) =>
            {
                UseNancyTenantConfiguration(app);
            })
            .Builder.UseStaticFiles();
        }

        public static void UseNancyTenantConfiguration(IApplicationBuilder app)
        {
            var contextAccessor =
                app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();

            app.UseOwin(x => x.UseNancy(no =>
            {
                no.Bootstrapper = new ModularNancyBootstrapper(
                    new[] {
                        (IAssemblyCatalog)new DependencyContextAssemblyCatalog(),
                        (IAssemblyCatalog)new AmbientAssemblyCatalog(contextAccessor)
                    });
            }));
        }
    }
}