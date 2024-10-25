using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Twitter.Settings;

namespace Microsoft.Extensions.DependencyInjection;

public static class OrchardCoreBuilderExtensions
{
    public static OrchardCoreBuilder ConfigureTwitterSettings(this OrchardCoreBuilder builder)
    {
        builder.ConfigureServices((tenantServices, serviceProvider) =>
        {
            var configuration = serviceProvider.GetRequiredService<IShellConfiguration>();
            var configurationSection = configuration.GetSection("OrchardCore_X");

            if (configurationSection.Value is null)
            {
                configurationSection = configuration.GetSection("OrchardCore_Twitter");
            }

            tenantServices.PostConfigure<TwitterSettings>(settings => configurationSection.Bind(settings));
        });

        return builder;
    }
}
