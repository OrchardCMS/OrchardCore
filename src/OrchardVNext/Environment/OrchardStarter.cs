using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using OrchardVNext.Environment.Configuration;
using OrchardVNext.Environment.Extensions;
using OrchardVNext.Environment.Extensions.Folders;
using OrchardVNext.Environment.Extensions.Loaders;
using OrchardVNext.Environment.ShellBuilders;
using OrchardVNext.FileSystems.AppData;
using OrchardVNext.FileSystems.Dependencies;
using OrchardVNext.FileSystems.VirtualPath;
using OrchardVNext.FileSystems.WebSite;
using OrchardVNext.Routing;

namespace OrchardVNext.Environment {
    public class OrchardStarter {
        private static void CreateHostContainer(IApplicationBuilder app) {
            app.UseServices(services => {

                services.AddSingleton<IHostEnvironment, DefaultHostEnvironment>();
                services.AddSingleton<IBuildManager, DefaultBuildManager>();
                services.AddSingleton<IAppDataFolderRoot, AppDataFolderRoot>();
                services.AddSingleton<IAssemblyLoader, DefaultAssemblyLoader>();
                services.AddSingleton<IAssemblyNameResolver, OrchardFrameworkAssemblyNameResolver>();


                services.AddSingleton<IWebSiteFolder, WebSiteFolder>();
                services.AddSingleton<IAppDataFolder, AppDataFolder>();
                services.AddSingleton<IDependenciesFolder, DefaultDependenciesFolder>();
                services.AddSingleton<IExtensionDependenciesManager, DefaultExtensionDependenciesManager>();
                services.AddSingleton<IAssemblyProbingFolder, DefaultAssemblyProbingFolder>();
                services.AddSingleton<IVirtualPathProvider, DefaultVirtualPathProvider>();

                services.AddTransient<IOrchardHost, DefaultOrchardHost>();
                {
                    services.AddSingleton<IShellSettingsManager, ShellSettingsManager>();

                    services.AddSingleton<IShellContextFactory, ShellContextFactory>();
                    {
                        services.AddSingleton<ICompositionStrategy, CompositionStrategy>();
                        {
                            services.AddSingleton<IExtensionLoaderCoordinator, ExtensionLoaderCoordinator>();
                            services.AddSingleton<IExtensionManager, ExtensionManager>();
                            {
                                services.AddSingleton<IExtensionHarvester, ExtensionHarvester>();
                                services.AddSingleton<IExtensionFolders, ModuleFolders>();
                                services.AddSingleton<IExtensionFolders, CoreModuleFolders>();
                                services.AddSingleton<IExtensionFolders, ThemeFolders>();

                                services.AddSingleton<IExtensionLoader, PrecompiledExtensionLoader>();
                                services.AddSingleton<IExtensionLoader, ReferencedExtensionLoader>();
                            }
                        }

                        services.AddSingleton<IShellContainerFactory, ShellContainerFactory>();
                    }
                };

                services.AddTransient<IOrchardShellHost, DefaultOrchardShellHost>();
                services.AddMvc();
            });

            app.UseMiddleware<OrchardContainerMiddleware>();
            app.UseMiddleware<OrchardShellHostMiddleware>();
            app.UseMiddleware<OrchardRouterMiddleware>();
        }

        public static IOrchardHost CreateHost(IApplicationBuilder app) {
            CreateHostContainer(app);

            return app.ApplicationServices.GetService<IOrchardHost>();
        }
    }
}
