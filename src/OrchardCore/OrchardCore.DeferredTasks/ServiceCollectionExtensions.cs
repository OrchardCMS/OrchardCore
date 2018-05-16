using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Modules;

namespace OrchardCore.DeferredTasks
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds tenant level deferred tasks services.
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection WithDeferredTasks(this IServiceCollection services)
        {
            return services.ConfigureTenantServices((collection) =>
            {
                collection.AddDeferredTasks();
            });
        }

        public static IServiceCollection AddDeferredTasks(this IServiceCollection services)
        {
            services.TryAddScoped<IDeferredTaskEngine, DeferredTaskEngine>();
            services.TryAddScoped<IDeferredTaskState, HttpContextTaskState>();
            return services;
        }
    }
}
