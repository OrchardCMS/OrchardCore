using Microsoft.Framework.DependencyInjection;
using Orchard.Abstractions.Environment;

namespace Orchard.Hosting {
    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddConsoleHost([NotNull] this IServiceCollection services) {
            return services.AddHost(internalServices => {
                internalServices.AddLogging();
                internalServices.AddOptions();

                internalServices.AddHostCore();

                internalServices.AddSingleton<IHostEnvironment, ConsoleHostEnvironment>();
            });
        }
    }
}