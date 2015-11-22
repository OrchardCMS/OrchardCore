using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment;
using Orchard.Environment.Commands;
using Orchard.FileSystem;

namespace Orchard.Hosting
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWebHost(this IServiceCollection services)
        {
            return services.AddHost(internalServices =>
            {
                internalServices.AddLogging();
                internalServices.AddOptions();

                internalServices.AddHostCore();
                internalServices.AddExtensionManager();
                internalServices.AddCommands();

                internalServices.AddWebFileSystems();

                internalServices.AddSingleton<IHostEnvironment, WebHostEnvironment>();
            });
        }
    }
}