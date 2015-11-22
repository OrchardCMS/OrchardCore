using Microsoft.Extensions.DependencyInjection;
using Orchard.FileSystem.AppData;

namespace Orchard.FileSystem
{
    public static class WebServiceCollectionExtensions
    {
        public static IServiceCollection AddFileSystems(this IServiceCollection services)
        {
            services.AddSingleton<IAppDataFolderRoot, AppDataFolderRoot>();
            services.AddSingleton<IAppDataFolder, PhysicalAppDataFolder>();

            return services;
        }
    }
}