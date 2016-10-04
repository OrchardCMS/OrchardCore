using Microsoft.Extensions.DependencyInjection;

namespace Orchard.Environment.Shell
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureShell(
            this IServiceCollection services,
            string shellsRootContainerName,
            string shellsContainerName)
        {
            return services.Configure<ShellOptions>(options =>
            {
                options.ShellsRootContainerName = shellsRootContainerName;
                options.ShellsContainerName = shellsContainerName;
            });
        }
    }
}