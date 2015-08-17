using Microsoft.Framework.DependencyInjection;
using OrchardVNext.FileSystem.AppData;
using OrchardVNext.FileSystem.VirtualPath;
using OrchardVNext.FileSystem.WebSite;

namespace OrchardVNext.FileSystem {
    public static class WebServiceCollectionExtensions {
        public static IServiceCollection AddFileSystems([NotNull] this IServiceCollection services) {
            services.AddSingleton<IAppDataFolderRoot, AppDataFolderRoot>();

            services.AddSingleton<IWebSiteFolder, WebSiteFolder>();
            services.AddSingleton<IAppDataFolder, AppDataFolder>();
            services.AddSingleton<IVirtualPathProvider, DefaultVirtualPathProvider>();

            return services;
        }

    }
}