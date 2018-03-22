using Microsoft.Extensions.Hosting;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ModularBackgroundServiceCollectionExtensions
    {
        public static IServiceCollection AddBackgroundService(this IServiceCollection services)
        {
            return services
                .AddSingleton<ModularBackgroundService>()
                .AddSingleton<IHostedService>(sp => sp.GetRequiredService<ModularBackgroundService>())
                .AddSingleton<IShellDescriptorManagerEventHandler>(sp => sp.GetRequiredService<ModularBackgroundService>());
        }
    }
}
