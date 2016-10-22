using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Extensions.Info.Extensions;
using Orchard.Environment.Extensions.Info.Manifests;

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