using Microsoft.Extensions.DependencyInjection;

namespace Orchard.Environment.Shell
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureShell(
            this IServiceCollection services,
            string shellLocation)
        {
            return services.Configure<ShellOptions>(options =>
            {
                options.Location = shellLocation;
            });
        }
    }
}