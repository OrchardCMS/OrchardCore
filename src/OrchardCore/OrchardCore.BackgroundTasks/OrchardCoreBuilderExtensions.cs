using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.BackgroundTasks;
using OrchardCore.Modules;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level background tasks services.
        /// </summary>
        public static OrchardCoreBuilder AddBackgroundTasks(this OrchardCoreBuilder builder)
        {
            builder.ConfigureServices((services, sp) =>
            {
                services.TryAddSingleton<IBackgroundTaskService, BackgroundTaskService>();
                services.AddScoped<BackgroundTasksStarter>();
                services.AddScoped<IModularTenantEvents>(serviceProvider => serviceProvider.GetRequiredService<BackgroundTasksStarter>());
            });

            return builder;
        }
    }
}
