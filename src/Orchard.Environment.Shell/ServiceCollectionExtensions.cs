using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Shell.Descriptor;
using Orchard.Environment.Shell.Descriptor.Settings;

namespace Orchard.Environment.Shell
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMultiTenancy(
            this IServiceCollection services,
            string shellLocation)
        {
            services.AddSingleton<IShellSettingsManager, ShellSettingsManager>();
            services.AddScoped<IShellDescriptorManager, AllFeaturesShellDescriptorManager>();

            services.Configure<ShellOptions>(options =>
            {
                options.Location = shellLocation;
            });

            return services;
        }
    }
}