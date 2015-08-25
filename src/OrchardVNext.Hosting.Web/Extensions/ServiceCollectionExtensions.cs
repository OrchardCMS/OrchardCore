using Microsoft.Framework.DependencyInjection;
using OrchardVNext.Abstractions.Environment;
using OrchardVNext.FileSystem;

namespace OrchardVNext.Hosting {
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