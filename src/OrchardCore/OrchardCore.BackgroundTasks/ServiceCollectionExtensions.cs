using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrchardCore.Environment.Shell;

namespace OrchardCore.BackgroundTasks
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBackgroundHostedService(this IServiceCollection services)
        {
            return services
                .AddSingleton<BackgroundHostedService>()
                .AddSingleton<IHostedService>(sp => sp.GetRequiredService<BackgroundHostedService>())
                .AddSingleton<IShellDescriptorManagerHostEventHandler>(sp => sp.GetRequiredService<BackgroundHostedService>());
        }
    }
}
