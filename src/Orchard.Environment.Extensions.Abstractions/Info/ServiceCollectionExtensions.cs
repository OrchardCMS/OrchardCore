using Microsoft.Extensions.DependencyInjection;

namespace Orchard.Environment.Extensions.Info
{
    public static class ServiceCollectionExtensions
    {
        public static void AddExtensions(
            this IServiceCollection services)
        {
            services.AddSingleton<IExtensionProvider, ExtensionProvider>();
            {
                services.AddSingleton<IManifestProvider, ManifestProvider>();
            }
        }
    }
}