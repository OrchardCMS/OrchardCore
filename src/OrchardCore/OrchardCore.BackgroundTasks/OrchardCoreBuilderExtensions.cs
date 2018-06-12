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
            builder.ConfigureServices(services =>
            {
                services.TryAddSingleton<IBackgroundTaskService, BackgroundTaskService>();
                services.AddScoped<BackgroundTasksStarter>();
                services.AddScoped<IModularTenantEvents>(sp => sp.GetRequiredService<BackgroundTasksStarter>());
            });

            return builder;
        }
    }
}
