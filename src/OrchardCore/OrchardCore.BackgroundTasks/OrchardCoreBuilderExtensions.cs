using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Modules;

namespace OrchardCore.BackgroundTasks
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level background tasks services.
        /// </summary>
        public static OrchardCoreBuilder AddBackgroundTasks(this OrchardCoreBuilder builder)
        {
            return builder.ConfigureTenantServices((collection) =>
            {
                AddBackgroundTasksTenantServices(collection);
            });
        }

        public static void AddBackgroundTasksTenantServices(IServiceCollection services)
        {
            services.TryAddSingleton<IBackgroundTaskService, BackgroundTaskService>();

            services.AddScoped<BackgroundTasksStarter>();
            services.AddScoped<IModularTenantEvents>(sp => sp.GetRequiredService<BackgroundTasksStarter>());
        }
    }
}
