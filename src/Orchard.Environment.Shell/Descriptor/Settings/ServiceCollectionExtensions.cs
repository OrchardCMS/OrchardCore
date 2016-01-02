using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Shell.Descriptor.Settings;

namespace Orchard.Environment.Shell
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureShell(
            this IServiceCollection services,
            string shellLocation)
        {
            return services.Configure<ShellDescriptorOptions>(options =>
            {
                options.ShellLocation = shellLocation;
            });
        }
    }
}