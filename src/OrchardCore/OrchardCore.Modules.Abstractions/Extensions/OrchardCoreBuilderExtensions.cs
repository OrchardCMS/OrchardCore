using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Modules.Extensions
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level configuration to serve static files from modules
        /// </summary>
        public static OrchardCoreBuilder UseStaticFiles(this OrchardCoreBuilder builder)
        {
            return builder.ConfigureTenant((app, routes, sp) =>
            {
                UseStaticFilesTenantConfiguration(app);
            });
        }

        public static void UseStaticFilesTenantConfiguration(IApplicationBuilder app)
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
        }

        /// <summary>
        /// Adds host level tag helper services.
        /// </summary>
        public static OrchardCoreBuilder AddTagHelpers(this OrchardCoreBuilder builder, string assemblyName)
        {
            return builder.AddTagHelpers(Assembly.Load(new AssemblyName(assemblyName)));
        }

        /// <summary>
        /// Adds host level tag helper services.
        /// </summary>
        public static OrchardCoreBuilder AddTagHelpers(this OrchardCoreBuilder builder, Assembly assembly)
        {
            builder.Services.AddTagHelpers(assembly);
            return builder;
        }
    }
}
