using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Orchard.DeferredTasks
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDeferredTasks(this IServiceCollection services)
        {
            services.TryAddScoped<IDeferredTaskEngine, DeferredTaskEngine>();
            services.TryAddScoped<IDeferredTaskState, HttpContextTaskState>();
            return services;
        }
    }
}
