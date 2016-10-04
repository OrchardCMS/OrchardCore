using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Orchard.Environment.Shell
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureShell(
            this IServiceCollection services,
            IFileProvider contentRootFileProvider,
            string shellContainerLocation)
        {
            return services.Configure<ShellOptions>(options =>
            {
                options.ContentRootFileProvider = contentRootFileProvider;
                options.ShellContainerLocation = shellContainerLocation;
            });
        }
    }
}