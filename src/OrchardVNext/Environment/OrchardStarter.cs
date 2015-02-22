using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Runtime;
using OrchardVNext.Environment.Configuration;
using OrchardVNext.Environment.Extensions;
using OrchardVNext.Environment.Extensions.Folders;
using OrchardVNext.Environment.Extensions.Loaders;
using OrchardVNext.Environment.ShellBuilders;
using OrchardVNext.FileSystems.AppData;
using OrchardVNext.FileSystems.VirtualPath;
using OrchardVNext.FileSystems.WebSite;
using OrchardVNext.Routing;

namespace OrchardVNext.Environment {
    public class OrchardStarter {
        private static void CreateHostContainer(IApplicationBuilder app) {
            app.UseServices(services => {
                services.AddSingleton<IHostEnvironment, DefaultHostEnvironment>();
                services.AddSingleton<IAppDataFolderRoot, AppDataFolderRoot>();

                services.AddSingleton<IWebSiteFolder, WebSiteFolder>();
                services.AddSingleton<IAppDataFolder, AppDataFolder>();
                services.AddSingleton<IVirtualPathProvider, DefaultVirtualPathProvider>();

                services.AddSingleton<ILoggerFactory, TestLoggerFactory>();

                // Caching - Move out
                services.AddInstance<ICacheContextAccessor>(new CacheContextAccessor());
                services.AddSingleton<ICache, Cache>();

                services.AddTransient<IOrchardHost, DefaultOrchardHost>();
                {
                    services.AddSingleton<IShellSettingsManager, ShellSettingsManager>();

                    services.AddSingleton<IShellContextFactory, ShellContextFactory>();
                    {
                        services.AddSingleton<ICompositionStrategy, CompositionStrategy>();
                        {
                            services.AddSingleton<IOrchardLibraryManager, OrchardLibraryManager>();
                            services.AddSingleton<IExtensionManager, ExtensionManager>();
                            {
                                services.AddSingleton<IExtensionHarvester, ExtensionHarvester>();
                                services.AddSingleton<IExtensionFolders, CoreModuleFolders>();
                                services.AddSingleton<IExtensionFolders, ModuleFolders>();

                                services.AddSingleton<IExtensionLoader, CoreExtensionLoader>();
                                services.AddSingleton<IExtensionLoader, DynamicExtensionLoader>();
                            }
                        }

                        services.AddSingleton<IShellContainerFactory, ShellContainerFactory>();
                    }
                }
                
                services.AddTransient<IOrchardShellHost, DefaultOrchardShellHost>();
                
                services.AddInstance<IServiceManifest>(new ServiceManifest(services));
            });
            
            app.UseMiddleware<OrchardContainerMiddleware>();
            app.UseMiddleware<OrchardShellHostMiddleware>();

            // Think this needs to be inserted in a different part of the pipeline, possibly
            // dhen DI is created for the shell
            app.UseMiddleware<OrchardRouterMiddleware>();
        }

        public static IOrchardHost CreateHost(IApplicationBuilder app) {
            CreateHostContainer(app);

            return app.ApplicationServices.GetService<IOrchardHost>();
        }
    }

    public class ServiceManifest : IServiceManifest {
        public ServiceManifest(IServiceCollection fallback) {

            var manifestTypes = fallback.Where(t => t.ServiceType.GetTypeInfo().GenericTypeParameters.Length == 0
                    && t.ServiceType != typeof(IServiceManifest)
                    && t.ServiceType != typeof(IServiceProvider))
                    .Select(t => t.ServiceType).Distinct();

            Services = manifestTypes;
        }

        public IEnumerable<Type> Services { get; private set; }
    }
}