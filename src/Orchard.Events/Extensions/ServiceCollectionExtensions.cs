using Microsoft.Extensions.DependencyInjection;

namespace Orchard.Events {
    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddNotifierEvents(this IServiceCollection services) {
            services.AddScoped<IEventBus, DefaultOrchardEventBus>();
            services.AddScoped<IEventNotifier, DefaultOrchardEventNotifier>();

            return services;
        }
    }
}