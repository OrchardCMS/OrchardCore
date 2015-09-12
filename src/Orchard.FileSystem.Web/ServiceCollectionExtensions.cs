using Microsoft.Framework.DependencyInjection;
using Orchard.FileSystem.VirtualPath;
using Orchard.FileSystem.WebSite;

namespace Orchard.FileSystem {
    public static class WebServiceCollectionExtensions {
        public static IServiceCollection AddWebFileSystems([NotNull] this IServiceCollection services) {
            services.AddSingleton<IClientFolder, WebSiteFolder>();
            services.AddSingleton<IVirtualPathProvider, DefaultVirtualPathProvider>();

            return services;
        }

    }
}