using Microsoft.Framework.DependencyInjection;
using OrchardVNext.FileSystem.Client;
using OrchardVNext.FileSystem.VirtualPath;
using OrchardVNext.FileSystem.WebSite;

namespace OrchardVNext.FileSystem {
    public static class WebServiceCollectionExtensions {
        public static IServiceCollection AddWebFileSystems([NotNull] this IServiceCollection services) {
            services.AddSingleton<IClientFolder, WebSiteClientFolder>();
            services.AddSingleton<IVirtualPathProvider, DefaultVirtualPathProvider>();

            return services;
        }

    }
}