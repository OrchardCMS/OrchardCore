using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using OrchardCore.Modules;

namespace Microsoft.AspNetCore.Builder
{
    public static class ModularApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseModules(this IApplicationBuilder app, Action<IApplicationBuilder> modules = null)
        {
            var env = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();

            env.ContentRootFileProvider = new CompositeFileProvider(
                new ModuleEmbeddedFileProvider(env),
                env.ContentRootFileProvider);
            
            // Ensure the shell tenants are loaded when a request comes in
            // and replaces the current service provider for the tenant's one.
            app.UseMiddleware<PoweredByMiddleware>();
            app.UseMiddleware<ModularTenantContainerMiddleware>();

            app.ConfigureModules(modules);

            app.UseMiddleware<ModularTenantRouterMiddleware>();

            return app;
        }

        public static IApplicationBuilder ConfigureModules(this IApplicationBuilder app, Action<IApplicationBuilder> modules)
        {
            modules?.Invoke(app);
            return app;
        }

        public static IApplicationBuilder UseStaticFilesModules(this IApplicationBuilder app)
        {
            var env = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();

            IFileProvider fileProvider;
            if (env.IsDevelopment())
            {
                var fileProviders = new List<IFileProvider>();
                fileProviders.Add(new ModuleProjectStaticFileProvider(env));
                fileProviders.Add(new ModuleEmbeddedStaticFileProvider(env));
                fileProvider = new CompositeFileProvider(fileProviders);
            }
            else
            {
                fileProvider = new ModuleEmbeddedStaticFileProvider(env);
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = "",
                FileProvider = fileProvider
            });

            return app;
        }
    }
}