using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Orchard.BackgroundTasks
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBackgroundTasks(this IServiceCollection services)
        {
            services.TryAddSingleton<IBackgroundTaskService, BackgroundTaskService>();
            return services;
        }
    }
}
