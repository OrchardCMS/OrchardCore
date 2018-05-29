using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Nancy;
using Nancy.Owin;
using OrchardCore.Nancy.AssemblyCatalogs;

namespace OrchardCore.Nancy
{
    public static class TenantApplicationBuilderExtensions
    {

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