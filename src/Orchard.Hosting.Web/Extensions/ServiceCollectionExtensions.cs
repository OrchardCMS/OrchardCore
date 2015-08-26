using Microsoft.Framework.DependencyInjection;
using Orchard.Abstractions.Environment;
using Orchard.FileSystem;

namespace Orchard.Hosting {
    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddWebHost([NotNull] this IServiceCollection services) {
            return services.AddHost(internalServices => {
                internalServices.AddLogging();
                internalServices.AddOptions();

                internalServices.AddHostCore();

                internalServices.AddWebFileSystems();
                
                internalServices.AddSingleton<IHostEnvironment, WebHostEnvironment>();
            });
        }
    }
}