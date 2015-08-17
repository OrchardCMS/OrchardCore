using Microsoft.Framework.DependencyInjection;
using OrchardVNext.Abstractions.Environment;

namespace OrchardVNext.Hosting {
    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddWeb([NotNull] this IServiceCollection services) {
            services.AddHost(internalServices => {
                services.AddHostCore();
            });

            services.AddSingleton<IHostEnvironment, WebHostEnvironment>();
            return services;
        }
    }
}