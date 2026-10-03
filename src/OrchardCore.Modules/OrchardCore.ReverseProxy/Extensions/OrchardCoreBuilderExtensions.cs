using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.ReverseProxy.Settings;

namespace Microsoft.Extensions.DependencyInjection;

public static class OrchardCoreBuilderExtensions
{
    public static OrchardCoreBuilder ConfigureReverseProxySettings(this OrchardCoreBuilder builder)
    {
        builder.ConfigureServices((tenantServices, serviceProvider) =>
        {
            var configuration = serviceProvider.GetRequiredService<IShellConfiguration>();

            tenantServices.PostConfigure<ReverseProxySettings>(settings =>
            {
                // The 'OrchardCore_ReverseProxy' section is deprecated and will be removed in a future major version, use 'ReverseProxy' instead.
                configuration.GetSectionCompat("ReverseProxy", "OrchardCore_ReverseProxy").Bind(settings);

                settings.FromConfiguration = true;
            });
        });

        return builder;
    }
}
