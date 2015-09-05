using Microsoft.Framework.DependencyInjection;

namespace Orchard.Events {
    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddNotifierEvents([NotNull] this IServiceCollection services) {
            services.AddScoped<IEventBus, DefaultOrchardEventBus>();
            services.AddScoped<IEventNotifier, DefaultOrchardEventNotifier>();

            services.AddSingleton<INotifier, Notifier>();

            return services;
        }
    }
}