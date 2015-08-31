using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Notification;

namespace Orchard.Events {
    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddNotifierEvents([NotNull] this IServiceCollection services) {
            //services.AddNotifier();
            
            services.AddScoped<IEventBus, DefaultOrchardEventBus>();
            services.AddScoped<IEventNotifier, DefaultOrchardEventNotifier>();
            services.AddSingleton<INotifierMethodAdapter, TestProxyNotifierMethodAdapter>();
            services.AddSingleton<INotifier, InternalNotifier>();

            return services;
        }
    }
}