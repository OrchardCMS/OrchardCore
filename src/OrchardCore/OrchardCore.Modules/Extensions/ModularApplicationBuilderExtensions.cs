using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using OrchardCore.Environment.Extensions;
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
            var extensionManager = app.ApplicationServices.GetRequiredService<IExtensionManager>();
            var env = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();

                // TODO: configure the location and parameters (max-age) per module.
                var availableExtensions = extensionManager.GetExtensions();
            foreach (var extension in availableExtensions)
            {
                var contentSubPath = Path.Combine(extension.SubPath, "wwwroot");

                if (env.ContentRootFileProvider.GetDirectoryContents(contentSubPath).Exists)
                {
                    IFileProvider fileProvider;
                    if (env.IsDevelopment())
                    {
                        var fileProviders = new List<IFileProvider>();
                        fileProviders.Add(new ModuleProjectContentFileProvider(env, contentSubPath));
                        fileProviders.Add(new ModuleEmbeddedFileProvider(env, contentSubPath));
                        fileProvider = new CompositeFileProvider(fileProviders);
                    }
                    else
                    {
                        fileProvider = new ModuleEmbeddedFileProvider(env, contentSubPath);
                    }

                    app.UseStaticFiles(new StaticFileOptions
                    {
                        RequestPath = "/" + extension.Id,
                        FileProvider = fileProvider
                    });
                }
            }

            return app;
        }
    }
}