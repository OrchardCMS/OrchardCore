using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Modules;

namespace OrchardCore.DeferredTasks
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level deferred tasks services.
        /// </summary>
        public static OrchardCoreBuilder AddDeferredTasks(this OrchardCoreBuilder builder)
        {
            return builder.ConfigureServices((collection, sp) =>
            {
                AddDeferredTasksTenantServices(collection);
            });
        }

        public static IServiceCollection AddDeferredTasksTenantServices(this IServiceCollection services)
        {
            services.TryAddScoped<IDeferredTaskEngine, DeferredTaskEngine>();
            services.TryAddScoped<IDeferredTaskState, HttpContextTaskState>();
            return services;
        }
    }
}
