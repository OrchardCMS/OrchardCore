using Microsoft.Framework.DependencyInjection;
using Orchard.FileSystem.AppData;

namespace Orchard.FileSystem {
    public static class WebServiceCollectionExtensions {
        public static IServiceCollection AddFileSystems([NotNull] this IServiceCollection services) {
            services.AddSingleton<IAppDataFolderRoot, AppDataFolderRoot>();
            
            services.AddSingleton<IAppDataFolder, AppDataFolder>();

            return services;
        }

    }
}