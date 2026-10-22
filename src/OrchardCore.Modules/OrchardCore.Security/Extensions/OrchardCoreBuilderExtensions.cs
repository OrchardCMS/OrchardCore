using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Security.Settings;

namespace Microsoft.Extensions.DependencyInjection;

public static class OrchardCoreBuilderExtensions
{
    public static OrchardCoreBuilder ConfigureSecuritySettings(this OrchardCoreBuilder builder)
    {
        builder.ConfigureServices((tenantServices, serviceProvider) =>
        {
            var configuration = serviceProvider.GetRequiredService<IShellConfiguration>();

            tenantServices.PostConfigure<SecuritySettings>(settings =>
            {
                settings.ContentSecurityPolicy.Clear();
                settings.PermissionsPolicy.Clear();

                // The 'OrchardCore_Security' section is deprecated and will be removed in a future major version, use 'Security' instead.
                configuration.GetSectionCompat("Security", "OrchardCore_Security").Bind(settings);

                settings.FromConfiguration = true;
            });
        });

        return builder;
    }
}
