using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Runtime;
using OrchardVNext.Environment.Configuration;
using OrchardVNext.Environment.Extensions;
using OrchardVNext.Environment.Extensions.Folders;
using OrchardVNext.Environment.Extensions.Loaders;
using OrchardVNext.Environment.ShellBuilders;
using OrchardVNext.FileSystems.VirtualPath;
using OrchardVNext.FileSystems.WebSite;
using OrchardVNext.Routing;

namespace OrchardVNext.Environment {
    public class OrchardStarter {
        private static void CreateHostContainer(IApplicationBuilder app) {
            app.UseServices(services => {
                services.AddSingleton<IHostEnvironment, DefaultHostEnvironment>();

                services.AddSingleton<IWebSiteFolder, WebSiteFolder>();
                services.AddSingleton<IVirtualPathProvider, DefaultVirtualPathProvider>();

                services.AddSingleton<ILoggerFactory, TestLoggerFactory>();

                // Caching - Move out?
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
                                services.AddSingleton<IExtensionFolders, ModuleFolders>();
                                services.AddSingleton<IExtensionFolders, CoreModuleFolders>();

                                services.AddSingleton<IExtensionLoader, DefaultExtensionLoader>();
                            }
                        }

                        services.AddSingleton<IShellContainerFactory, ShellContainerFactory>();
                    }
                };

                services.AddTransient<IOrchardShellHost, DefaultOrchardShellHost>();
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
}