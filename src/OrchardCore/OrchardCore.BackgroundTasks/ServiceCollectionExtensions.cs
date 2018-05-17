using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Modules;

namespace OrchardCore.BackgroundTasks
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBackgroundTasks(this IServiceCollection services)
        {
            services.TryAddSingleton<IBackgroundTaskService, BackgroundTaskService>();

            services.AddScoped<BackgroundTasksStarter>();
            services.AddScoped<IModularTenantEvents>(sp => sp.GetRequiredService<BackgroundTasksStarter>());

            return services;
        }
    }
}
