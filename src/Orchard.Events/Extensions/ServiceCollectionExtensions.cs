using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.TelemetryAdapter;

namespace Orchard.Events {
    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddNotifierEvents([NotNull] this IServiceCollection services) {
            services.AddTelemetrySourceAdapter();

            services.AddScoped<IEventBus, DefaultOrchardEventBus>();
            services.AddScoped<IEventNotifier, DefaultOrchardEventNotifier>();

            services.AddSingleton<IMethodAdaptor, DefaultMethodAdaptor>();

            services.AddSingleton<TelemetrySourceAdapter, InternalTelemetrySourceAdapter>();

            return services;
        }
    }
}