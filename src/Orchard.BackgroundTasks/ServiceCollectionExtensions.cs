using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Orchard.Environment.Shell;

namespace Orchard.BackgroundTasks
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBackgroundTasks(this IServiceCollection services)
        {
            services.TryAddSingleton<IBackgroundTaskService, BackgroundTaskService>();

            services.AddScoped<BackgroundTasksStarter>();
            services.AddScoped<IOrchardShellEvents>(sp => sp.GetRequiredService<BackgroundTasksStarter>());

            return services;
        }
    }
}
