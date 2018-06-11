using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Modules;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds OrchardCore services to the host service collection.
        /// </summary>
        public static OrchardCoreBuilder AddOrchardCore(this IServiceCollection services)
        {
            // If an instance of OrchardCoreBuilder exists reuse it,
            // so we can call AddOrchardCore several times.
            var builder = services
                .LastOrDefault(d => d.ServiceType == typeof(OrchardCoreBuilder))?
                .ImplementationInstance as OrchardCoreBuilder;

            if (builder == null)
            {
                builder = new OrchardCoreBuilder(services);
                services.AddSingleton(builder);

                AddDefaultServices(services);
                AddShellServices(services);
                AddExtensionServices(builder);
                AddStaticFiles(builder);

                // Register the list of services to be resolved later on
                services.AddSingleton(services);
            }

            return builder;
        }

        private static void AddDefaultServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddOptions();

            // These services might be moved at a higher level if no components from OrchardCore needs them.
            services.AddLocalization();
            services.AddWebEncoders();

            // ModularTenantRouterMiddleware which is configured with UseModules() calls UseRouter() which requires the routing services to be
            // registered. This is also called by AddMvcCore() but some applications that do not enlist into MVC will need it too.
            services.AddRouting();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IClock, Clock>();
            services.AddScoped<ILocalClock, LocalClock>();

            services.AddSingleton<IPoweredByMiddlewareOptions, PoweredByMiddlewareOptions>();
            services.AddTransient<IModularTenantRouteBuilder, ModularTenantRouteBuilder>();
        }

        private static void AddShellServices(IServiceCollection services)
        {
            // Use a single tenant and all features by default
            services.AddHostingShellServices();
            services.AddAllFeaturesDescriptor();

            // Registers the application main feature
            services.AddTransient(sp => new ShellFeature
            (
                sp.GetRequiredService<IHostingEnvironment>().ApplicationName, alwaysEnabled: true)
            );
        }

        private static void AddExtensionServices(OrchardCoreBuilder builder)
        {
            builder.ApplicationServices.AddExtensionManagerHost();

            builder.ConfigureServices(services =>
            {
                services.AddExtensionManager();
            });
        }

        /// <summary>
        /// Adds tenant level configuration to serve static files from modules
        /// </summary>
        private static void AddStaticFiles(OrchardCoreBuilder builder)
        {
            builder.Configure((app, routes, serviceProvider) =>
            {
                var env = serviceProvider.GetRequiredService<IHostingEnvironment>();

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
            });
        }
    }
}
