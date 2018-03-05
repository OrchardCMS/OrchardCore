using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace OrchardCore.BackgroundTasks
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHostedBackgroundService(this IServiceCollection services)
        {
            services.TryAddSingleton<IHostedService, HostedBackgroundService>();
            return services;
        }
    }
}
