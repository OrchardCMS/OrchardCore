using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace OrchardCore.BackgroundTasks.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBackgroundTaskDocument(this IServiceCollection services)
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<BackgroundTaskOptions>,
                BackgroundTaskDocumentOptionsSetup>());

            return services;
        }
    }
}