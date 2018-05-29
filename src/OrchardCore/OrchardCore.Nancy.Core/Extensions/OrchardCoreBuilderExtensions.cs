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
            return builder.Startup.ConfigureServices((tenant, sp) =>
            {
                tenant.Services.AddRouting();
            })
            .Configure((tenant, routes, sp) =>
            {
                tenant.UseNancy();
            })
            .Builder.UseStaticFiles();
        }

        public static TenantApplicationBuilder UseNancy(this TenantApplicationBuilder tenant)
        {
            var contextAccessor = tenant.ApplicationBuilder
                .ApplicationServices.GetRequiredService<IHttpContextAccessor>();

            tenant.ApplicationBuilder.UseOwin(x => x.UseNancy(no =>
            {
                no.Bootstrapper = new ModularNancyBootstrapper(
                    new[] {
                        (IAssemblyCatalog)new DependencyContextAssemblyCatalog(),
                        (IAssemblyCatalog)new AmbientAssemblyCatalog(contextAccessor)
                    });
            }));

            return tenant;
        }
    }
}