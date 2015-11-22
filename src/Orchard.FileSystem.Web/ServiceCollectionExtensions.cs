using Microsoft.Extensions.DependencyInjection;
using Orchard.FileSystem.VirtualPath;
using Orchard.FileSystem.WebSite;

namespace Orchard.FileSystem
{
    public static class WebServiceCollectionExtensions
    {
        public static IServiceCollection AddWebFileSystems(this IServiceCollection services)
        {
            services.AddSingleton<IClientFolder, WebSiteFolder>();
            services.AddSingleton<IVirtualPathProvider, DefaultVirtualPathProvider>();

            return services;
        }
    }
}