using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Security;
using OrchardCore.Security.Settings;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        public static OrchardCoreBuilder ConfigureSecuritySettings(this OrchardCoreBuilder builder)
        {
            builder.ConfigureServices((tenantServices, serviceProvider) =>
            {
                var configurationSection = serviceProvider.GetRequiredService<IShellConfiguration>().GetSection("OrchardCore_Security");

                tenantServices.PostConfigure<SecuritySettings>(settings =>
                {
                    // Reset the settings to avoid merging with the current settings values
                    settings.ContentSecurityPolicy = SecurityHeaderDefaults.ContentSecurityPolicy;
                    //settings.PermissionsPolicy = SecurityHeaderDefaults.PermissionsPolicy;

                    configurationSection.Bind(settings);

                    settings.FromConfiguration = true;
                });
            });

            return builder;
        }
    }
}
