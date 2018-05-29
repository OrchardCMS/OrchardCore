using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.BackgroundTasks;
using OrchardCore.Modules;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TenantServicesBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level background tasks services.
        /// </summary>
        public static TenantServicesBuilder AddBackgroundTasks(this TenantServicesBuilder tenant)
        {
            var services = tenant.Services;

            services.TryAddSingleton<IBackgroundTaskService, BackgroundTaskService>();
            services.AddScoped<BackgroundTasksStarter>();
            services.AddScoped<IModularTenantEvents>(sp => sp.GetRequiredService<BackgroundTasksStarter>());

            return tenant;
        }
    }
}