using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using OrchardCore;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Localization;
using OrchardCore.Modules;
using OrchardCore.Modules.FileProviders;

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

                AddAntiForgery(builder);
                AddAuthentication(builder);
                AddDataProtection(builder);

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

            // For performance, prevents the 'ResourceManagerStringLocalizer' from being used.
            services.AddSingleton<IStringLocalizerFactory, NullStringLocalizerFactory>();

            services.AddWebEncoders();

            // ModularTenantRouterMiddleware which is configured with UseOrchardCore() calls UseRouter() which requires the routing services to be
            // registered. This is also called by AddMvcCore() but some applications that do not enlist into MVC will need it too.
            services.AddRouting();

            services.AddHttpContextAccessor();
            services.AddSingleton<IClock, Clock>();
            services.AddScoped<ILocalClock, LocalClock>();

            services.AddScoped<ILocalizationService, DefaultLocalizationService>();
            services.AddScoped<ICalendarManager, DefaultCalendarManager>();
            services.AddScoped<ICalendarSelector, DefaultCalendarSelector>();

            services.AddSingleton<IPoweredByMiddlewareOptions, PoweredByMiddlewareOptions>();
            services.AddTransient<IModularTenantRouteBuilder, ModularTenantRouteBuilder>();

            services.AddScoped<IOrchardHelper, DefaultOrchardHelper>();
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
            builder.ApplicationServices.AddSingleton<IModuleNamesProvider, AssemblyAttributeModuleNamesProvider>();
            builder.ApplicationServices.AddSingleton<IApplicationContext, ModularApplicationContext>();

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
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IModuleStaticFileProvider>(serviceProvider =>
                {
                    var env = serviceProvider.GetRequiredService<IHostingEnvironment>();
                    var appContext = serviceProvider.GetRequiredService<IApplicationContext>();

                    IModuleStaticFileProvider fileProvider;
                    if (env.IsDevelopment())
                    {
                        var fileProviders = new List<IStaticFileProvider>
                        {
                            new ModuleProjectStaticFileProvider(appContext),
                            new ModuleEmbeddedStaticFileProvider(appContext)
                        };
                        fileProvider = new ModuleCompositeStaticFileProvider(fileProviders);
                    }
                    else
                    {
                        fileProvider = new ModuleEmbeddedStaticFileProvider(appContext);
                    }
                    return fileProvider;
                });

                services.AddSingleton<IStaticFileProvider>(serviceProvider =>
                {
                    return serviceProvider.GetRequiredService<IModuleStaticFileProvider>();
                });
            });

            builder.Configure((app, routes, serviceProvider) =>
            {
                var fileProvider = serviceProvider.GetRequiredService<IModuleStaticFileProvider>();

                var options = serviceProvider.GetRequiredService<IOptions<StaticFileOptions>>().Value;

                options.RequestPath = "";
                options.FileProvider = fileProvider;

                var shellConfiguration = serviceProvider.GetRequiredService<IShellConfiguration>();

                var cacheControl = shellConfiguration.GetValue("StaticFileOptions:CacheControl", "public, max-age=2592000, s-max-age=31557600");

                // Cache static files for a year as they are coming from embedded resources and should not vary
                options.OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers[HeaderNames.CacheControl] = cacheControl;
                };

                app.UseStaticFiles(options);
            });
        }

        /// <summary>
        /// Adds host and tenant level antiforgery services.
        /// </summary>
        private static void AddAntiForgery(OrchardCoreBuilder builder)
        {
            builder.ApplicationServices.AddAntiforgery();

            builder.ConfigureServices((services, serviceProvider) =>
            {
                var settings = serviceProvider.GetRequiredService<ShellSettings>();

                var cookieName = "orchantiforgery_" + settings.Name;

                // If uninitialized, we use the host services.
                if (settings.State == TenantState.Uninitialized)
                {
                    // And delete a cookie that may have been created by another instance.
                    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                    httpContextAccessor.HttpContext.Response.Cookies.Delete(cookieName);
                    return;
                }

                // Re-register the antiforgery  services to be tenant-aware.
                var collection = new ServiceCollection()
                    .AddAntiforgery(options =>
                    {
                        options.Cookie.Name = cookieName;

                        // Don't set the cookie builder 'Path' so that it uses the 'IAuthenticationFeature' value
                        // set by the pipeline and comming from the request 'PathBase' which already ends with the
                        // tenant prefix but may also start by a path related e.g to a virtual folder.
                    });

                services.Add(collection);
            });
        }

        /// <summary>
        /// Adds host and tenant level authentication services and configuration.
        /// </summary>
        private static void AddAuthentication(OrchardCoreBuilder builder)
        {
            builder.ApplicationServices.AddAuthentication();

            builder.ConfigureServices(services =>
            {
                services.AddAuthentication();

                // IAuthenticationSchemeProvider is already registered at the host level.
                // We need to register it again so it is taken into account at the tenant level
                // because it holds a reference to an underlying dictionary, responsible of storing 
                // the registered schemes which need to be distinct for each tenant.
                services.AddSingleton<IAuthenticationSchemeProvider, AuthenticationSchemeProvider>();

            })
            .Configure(app =>
            {
                app.UseAuthentication();
            });
        }

        /// <summary>
        /// Adds tenant level data protection services.
        /// </summary>
        private static void AddDataProtection(OrchardCoreBuilder builder)
        {
            builder.ConfigureServices((services, serviceProvider) =>
            {
                var settings = serviceProvider.GetRequiredService<ShellSettings>();
                var options = serviceProvider.GetRequiredService<IOptions<ShellOptions>>();

                var directory = Directory.CreateDirectory(Path.Combine(
                    options.Value.ShellsApplicationDataPath,
                    options.Value.ShellsContainerName,
                    settings.Name, "DataProtection-Keys"));

                // Re-register the data protection services to be tenant-aware so that modules that internally
                // rely on IDataProtector/IDataProtectionProvider automatically get an isolated instance that
                // manages its own key ring and doesn't allow decrypting payloads encrypted by another tenant.
                // By default, the key ring is stored in the tenant directory of the configured App_Data path.
                var collection = new ServiceCollection()
                    .AddDataProtection()
                    .PersistKeysToFileSystem(directory)
                    .SetApplicationName(settings.Name)
                    .Services;

                // Retrieve the implementation type of the newly startup filter registered as a singleton
                var startupFilterType = collection.FirstOrDefault(s => s.ServiceType == typeof(IStartupFilter))?.GetImplementationType();

                if (startupFilterType != null)
                {
                    // Remove any previously registered data protection startup filters.
                    var descriptors = services.Where(s => s.ServiceType == typeof(IStartupFilter) &&
                        (s.GetImplementationType() == startupFilterType)).ToArray();

                    foreach (var descriptor in descriptors)
                    {
                        services.Remove(descriptor);
                    }
                }

                // Remove any previously registered options setups.
                services.RemoveAll<IConfigureOptions<KeyManagementOptions>>();
                services.RemoveAll<IConfigureOptions<DataProtectionOptions>>();

                services.Add(collection);
            });
        }
    }
}
