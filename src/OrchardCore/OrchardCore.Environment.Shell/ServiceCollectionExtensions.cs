using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Descriptor;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Environment.Shell.Descriptor.Settings;

namespace OrchardCore.Environment.Shell
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAllFeaturesDescriptor(this IServiceCollection services)
        {
            services.AddScoped<IShellDescriptorManager, AllFeaturesShellDescriptorManager>();

            return services;
        }

        public static IServiceCollection AddSetFeaturesDescriptor(this IServiceCollection services)
        {
            services.AddSingleton<IShellDescriptorManager>(sp =>
            {
                var shellFeatures = sp.GetServices<ShellFeature>();
                return new SetFeaturesShellDescriptorManager(shellFeatures);
            });

            return services;
        }
    }
}