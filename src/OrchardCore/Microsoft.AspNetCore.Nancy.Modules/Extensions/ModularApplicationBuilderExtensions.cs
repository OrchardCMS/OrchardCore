using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Nancy.Modules.AssemblyCatalogs;
using Microsoft.Extensions.DependencyInjection;
using Nancy;
using Nancy.Owin;

namespace Microsoft.AspNetCore.Nancy.Modules
{
    public static class ModularApplicationBuilderExtensions
    {
        public static ModularApplicationBuilder UseNancyModules(this ModularApplicationBuilder modularApp)
        {
            modularApp.Configure(app =>
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
            });

            return modularApp;
        }
    }
}