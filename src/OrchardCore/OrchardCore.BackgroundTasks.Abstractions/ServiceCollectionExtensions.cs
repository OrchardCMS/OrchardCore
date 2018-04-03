using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.BackgroundTasks
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBackgroundTaskAttributes(this IServiceCollection services)
        {
            return services.Configure<BackgroundTaskOptions>(options =>
            {
                options.SettingsProviders.Add(new BackgroundTaskAttributeSettingsProvider());
            });
        }
    }
}