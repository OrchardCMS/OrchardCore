using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.ResourceManagement;
using OrchardCore.Settings;

namespace OrchardCore.Resources;
public static class OrchardCoreBuilderExtensions
{
    public static OrchardCoreBuilder ConfigureResourceSettings(this OrchardCoreBuilder builder)
    {
        builder.ConfigureServices((tenantServices, serviceProvider) =>
        {
            var configurationSection = serviceProvider.GetRequiredService<IShellConfiguration>().GetSection("OrchardCore_Resources");

            tenantServices.PostConfigure<ResourceOptions>(settings =>
            {
                configurationSection.Bind(settings);

                settings.ResourceDebugMode = ResourceDebugMode.FromConfiguration;
            });
        });

        return builder;
    }
}
