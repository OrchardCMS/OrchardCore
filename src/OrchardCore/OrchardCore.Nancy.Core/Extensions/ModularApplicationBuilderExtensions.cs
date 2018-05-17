using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Nancy;
using Nancy.Owin;
using OrchardCore.Modules;
using OrchardCore.Nancy.AssemblyCatalogs;

namespace OrchardCore.Nancy
{
    public static class ModularApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level Nancy services.
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection WithNancy(this IServiceCollection services)
        {
            return services.ConfigureTenantServices(collection =>
            {
                collection.AddRouting();
            })
            .ConfigureTenant((app, routes, sp) =>
            {
                app.UseNancyModules();
                app.UseStaticFilesModules();
            });
        }

        public static IApplicationBuilder UseNancyModules(this IApplicationBuilder app)
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

            return app;
        }
    }
}