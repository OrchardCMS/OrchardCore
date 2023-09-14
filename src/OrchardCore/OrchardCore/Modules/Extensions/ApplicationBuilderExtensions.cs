using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using OrchardCore.Modules;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Enables multi-tenant requests support for the current path.
        /// </summary>
        public static IApplicationBuilder UseOrchardCore(this IApplicationBuilder app, Action<IApplicationBuilder> configure = null)
        {
            var env = app.ApplicationServices.GetRequiredService<IHostEnvironment>();
            var appContext = app.ApplicationServices.GetRequiredService<IApplicationContext>();

            env.ContentRootFileProvider = new CompositeFileProvider(
                new ModuleEmbeddedFileProvider(appContext),
                env.ContentRootFileProvider);

            // Init also the web host 'ContentRootFileProvider'.
            app.ApplicationServices.GetRequiredService<IWebHostEnvironment>()
                .ContentRootFileProvider = env.ContentRootFileProvider;

            app.UseMiddleware<PoweredByMiddleware>();

            // Ensure the shell tenants are loaded when a request comes in
            // and replaces the current service provider for the tenant's one.
            app.UseMiddleware<ModularTenantContainerMiddleware>();

            configure?.Invoke(app);

            app.UseMiddleware<ModularTenantRouterMiddleware>();

            return app;
        }
    }
}
